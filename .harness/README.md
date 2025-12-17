# Harness Config

This folder holds a minimal pipeline skeleton (`api-ci.yaml`).

Before running in Harness, replace placeholders:
- `REPLACE_ME_ORG`, `REPLACE_ME_PROJECT`
- `REPLACE_ME_GIT_CONNECTOR`
- `REPLACE_ME_AWS_CONNECTOR`

Then run the pipeline in Harness to build/test and push to ECR.

Tip: if you don't have ECR yet, create it in the AWS console (ECR > Create repository) or point the pipeline to Docker Hub for a first pass.
