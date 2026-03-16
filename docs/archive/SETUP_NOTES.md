# Setup Notes

## AWS

- Keep monthly budget under $20
- [x] Try Beanstalk - it worked but i want a little more control
- [x] Try EC2 - took awhile, but eventually got the API swagger up. (VPC issues)
- [x] Try ECS - now using docker containers on ECS Fargate.
- [ ] Setup Cognito user pool + app client for API JWT auth.

## Harness

- Connect GitHub, Docker and AWS accounts.
- Want to build and deploy to AWS.
- ~~Want to also setup and tear down AWS infrastructure when not being used.~~
- setup build, deploy and pause pieline. Pause will set the ECS TAsks to 0 `desiredCount`

## GitHub

## Docker Hub

## Local Development

- Use VSCode to build APIs and Lambdas using C# and Python
- Linux box for local test deployment
- Docker + MicroK8s installed
- VS Code for all projects
- Use Cognito-issued bearer tokens in Swagger/Postman for secured endpoint testing (planned).
