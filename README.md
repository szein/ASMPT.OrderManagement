# Introduction 
ASMPT Order managment Application

# Getting Started
1.	Installate dependencies
2.	Installate Software dependencies (docker, WSL for windwos ..etc)


# Build and Run Containers

Before you run docker commands:
**Make sure that you are in the root folder of the repository.**



# API

## API ENVIRONMENT Values:
Use the  **ASPNETCORE_ENVIRONMENT** variale to set one of the following 
- Development -> run it localy **without** database prsistance
- Staging -> run it localy **with** database prsistance
- Production or (none) -> for prduction secnario

```
 docker build -f api/Dockerfile -t api . 

 docker run --rm -d -p 8088:8080 -e ASPNETCORE_ENVIRONMENT=Staging -v sqlite-data:/ASMPT.stage/api/data --name api-dev -t api
```


 # Web
 Run the following command to start the web application

 ```
 docker build -f web/Dockerfile -t web .

 docker run --rm -d -p 8008:80 --name web-dev -t web
 ```




