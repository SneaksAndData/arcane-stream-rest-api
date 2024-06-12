# Quickstart Guide

## Prerequisites

This guide assumes you have the Kubernetes cluster version 1.29 or higher and the Arcane Operator version
0.0.1 or higher installed in your cluster.

This plugin supports only Amazon S3 compatible storages and authentication using AWS access key and secret key.

## Installation

1. Create a S3 bucket in your AWS account.
2. Create an IAM user with the following permissions:
    - `s3:PutObject`

3. Create a Kubernetes secret with the following keys:
```bash
$ kubectl create secret generic s3-secret \
    --from-literal=ARCANE.STREAM.RESTAPI__AWS_ACCESS_KEY_ID=<access-key-id> \
    --from-literal=ARCANE.STREAM.RESTAPI__AWS_ENDPOINT_URL=https://s3.<region>.amazonaws.com \
    --from-literal=ARCANE.STREAM.RESTAPI__AWS_SECRET_ACCESS_KEY=<access-key-secret>
 ``` 

4. Deploy the Arcane Stream plugin:
```bash
$ helm install arcane-stream-rest-api oci://ghcr.io/sneaksanddata/helm/arcane-stream-rest-api \
  --version v0.0.1 \
  --set jobTemplateSettings.extraEnvFrom[0].secretRef.name=s3-secret \
  --namespace arcane
```

5. Verify the installation
To verify the plugin installation run the following command:
```bash
$ kubectl get stream-classes --namespace arcane -l app.kubernetes.io/name=arcane-stream-rest-api
```
It should produce the similar output:
```
NAME                              PLURAL     APIGROUPREF                   PHASE
arcane-stream-rest-api-fa         api-fas    streaming.sneaksanddata.com   READY
arcane-stream-rest-api-paged-da   api-pdas   streaming.sneaksanddata.com   READY
arcane-stream-rest-api-paged-fa   api-pfas   streaming.sneaksanddata.com   READY
```

6. Create a stream definition:
```bash
export SINK_LOCATION=s3a://esd-esd-sandbox-default-stream-storage/openaq

cat <<EOF | envsubst | kubectl apply -f -
apiVersion: streaming.sneaksanddata.com/v1beta1
kind: RestApiPagedFixedAuth
metadata:
  name: openaq-air-quality-worlwide
  namespace: arcane
spec:
  apiSchemaEncoded: eyJwcm9wZXJ0aWVzIjp7InJlc3VsdHMiOnsiaXRlbXMiOnsicHJvcGVydGllcyI6eyJjaXR5Ijp7InR5cGUiOiJzdHJpbmcifSwiY29vcmRpbmF0ZXMiOnsicHJvcGVydGllcyI6eyJsYXRpdHVkZSI6eyJ0eXBlIjoibnVtYmVyIn0sImxvbmdpdHVkZSI6eyJ0eXBlIjoibnVtYmVyIn19LCJ0eXBlIjoib2JqZWN0In0sImNvdW50cnkiOnsidHlwZSI6InN0cmluZyJ9LCJkYXRlIjp7InByb3BlcnRpZXMiOnsibG9jYWwiOnsidHlwZSI6InN0cmluZyJ9LCJ1dGMiOnsidHlwZSI6InN0cmluZyJ9fSwidHlwZSI6Im9iamVjdCJ9LCJlbnRpdHkiOnsidHlwZSI6InN0cmluZyJ9LCJpc0FuYWx5c2lzIjp7InR5cGUiOiJib29sZWFuIn0sImlzTW9iaWxlIjp7InR5cGUiOiJib29sZWFuIn0sImxvY2F0aW9uIjp7InR5cGUiOiJzdHJpbmcifSwibG9jYXRpb25JZCI6eyJ0eXBlIjoiaW50ZWdlciJ9LCJwYXJhbWV0ZXIiOnsidHlwZSI6InN0cmluZyJ9LCJzZW5zb3JUeXBlIjp7InR5cGUiOiJzdHJpbmcifSwidW5pdCI6eyJ0eXBlIjoic3RyaW5nIn0sInZhbHVlIjp7InR5cGUiOiJudW1iZXIifX0sInR5cGUiOiJvYmplY3QifSwidHlwZSI6ImFycmF5In19LCJ0eXBlIjoib2JqZWN0In0=
  backFillStartDate: 1715172779000
  bodyTemplate: ''
  changeCaptureIntervalSeconds: 180
  credentialSecretRef:
    name: fake-auth
  groupingIntervalSeconds: 15
  httpMethod: GET
  httpTimeout: 120
  internalRateLimitCount: 100
  internalRateLimitInterval: 5
  jobTemplateRef:
    apiGroup: streaming.sneaksanddata.com
    kind: StreamingJobTemplate
    name: arcane-stream-rest-api
  lookBackInterval: 86400
  pageResolverConfiguration:
    resolverPropertyKeyChain:
      - results
    resolverType: OFFSET
    responseSize: 1
    startOffset: 1
  reloadingJobTemplateRef:
    apiGroup: streaming.sneaksanddata.com
    kind: StreamingJobTemplate
    name: arcane-stream-rest-api
  responsePropertyKeyChain:
    - results
  rowsPerGroup: 10000
  
  # The sink location should be in format: s3a://<bucket-name>/<path>
  sinkLocation: ${SINK_LOCATION}
  templatedFields:
    - fieldName: dateFrom
      fieldType: FILTER_DATE_BETWEEN_FROM
      formatString: yyyy-MM-ddTHH:mm:ssZ
      placement: URL
    - fieldName: dateTo
      fieldType: FILTER_DATE_BETWEEN_TO
      formatString: yyyy-MM-ddTHH:mm:ssZ
      placement: URL
    - fieldName: page
      fieldType: RESPONSE_PAGE
      formatString: ''
      placement: URL
  urlTemplate: >-
    https://api.openaq.org/v2/measurements?date_from=@dateFrom&date_to=@dateTo&limit=100&sort=desc&radius=1000&order_by=datetime&page=@page
```

6. Soon you should see the Kubernetes Job with name `openaq-air-quality-worlwide` in the `arcane` namespace and the stream definition in the `RELOADING` phase:
```bash
$ kubectl get jobs -n arcane
NAME                              COMPLETIONS   DURATION   AGE
openaq-air-quality-worlwide       0/1           0s         0s

$ kubectl get api-pdas -n arcane 
NAME                              SINK LOCATION    REFRESH  INTERVAL              PHASE 
openaq-air-quality-worlwide       api-pdas          streaming.sneaksanddata.com   RELOADING
```
