# Installation Guide

Follow these steps to successfully set up and run the web application:

## Clone Repository
Open Windows PowerShell and write down this commands one by one:
```
git clone https://github.com/NobleChad/LunaEdgeTask
```
```
cd LunaEdgeTask
```
## Apply Migrations
Once you open the app run the following commands
```
dotnet tool install --global dotnet-ef
```
```
dotnet ef database update
```
## Run the Application
Open Visual Studio and run the app. The default path should be C:\Users\USER_NAME\LunaEdgeTask. At the top, change debug option from htpps to Cotainer (Dockerfile).

# How to use
Create a user using register and then login to gain JWT token(Password must meet criterias). After this you can use task endpoints by writing down token in corresponding text field. You can get/post/put/delete tasks (Note, only creator of a task can modify it).
