# Auth0 Apps Manager

Sample applcation to list up the applications and their assocaited rules in the Auth0 based on AntDesign Blazor.

## Screenshots
![Screenshot](images/Screenshot001.png)
![Screenshot](images/Screenshot002.png)

## How to setup

### Preparation
1) [Create and authorize a machine-to-machine application](https://auth0.com/docs/tokens/management-api-access-tokens/create-and-authorize-a-machine-to-machine-application) for "Auth0 Apps Manager"
to call Auth0 Management API.

2) Assign "read:clients", "read:rules" permissions for the Auth0 Management API.
![Screenshot](images/assignpermissions.png)

3) Create another Regular Web Application with name AppsManager to authenticate/authorize your users to access the "Auth0 Apps Manager".
![Screenshot](images/createAppsManager.png)

4) Add https://localhost:5001/callback to the *Allowed Callback URLs* and add https://localhost:5001 to the *Allowed Logout URLs*.

### Installing
1. [Download](https://dotnet.microsoft.com/download/dotnet/5.0) and Install .NET SDK 5.0 or above.
2. Download, fork, or clone the repository.
3. Open the project with your favorite IDE (VS Code, Visual Studio, Atom, etc).
4. Update Auth0 and ApiSettings section in the appsettings.json. You can get the informaiotn for ApiSettings from the application created in Preparation step 1, and Auth0 setting from the application created in Preparation step 3.
#### Auth0
![Screenshot](images/auth0Settings.png)

#### ApiSettings
![Screenshot](images/apiSettings.png)

5. Run the app with your IDE or these commands:

```
$ cd /your-local-path/auth0app/src/
$ dotnet run
```
Then you can open `https://localhost:5001` with your browser.

### Secure Apps Manager Access
1. [Create a new rule](https://auth0.com/docs/rules/create-rules)
2. Update the script as below.
```js
function userWhitelistForSpecificApp(user, context, callback) {
  // Access should only be granted to verified users.
  if (!user.email || !user.email_verified) {
    return callback(new UnauthorizedError('Access denied.'));
  }

  // only enforce for AppsManager
  // bypass this rule for all other apps
  if (context.clientName !== 'AppsManager') {
    return callback(null, user, context);
  }

  const whitelist = ['liminghao0922@gmail.com']; // add authorized users here
  const userHasAccess = whitelist.some(function (email) {
    return email === user.email;
  });

  if (!userHasAccess) {
    return callback(new UnauthorizedError('Access denied.'));
  }

  callback(null, user, context);
}
```