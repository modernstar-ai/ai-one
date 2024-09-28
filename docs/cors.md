# Cors

## CORS with Azure App Service (Production)
When your .NET API is hosted on Azure App Service, you may also need to adjust the CORS settings from the Azure Portal, though the configuration in Program.cs should be sufficient in most cases.

Navigate to your Azure App Service in the Azure Portal.
Under Settings, go to CORS.
Add your allowed frontend domain(s) (e.g., https://your-production-domain.com).
Save the settings.

## Verifying CORS Requests

To verify that CORS is working, open your browser’s developer tools (F12), go to the Network tab, and inspect the API request. 

If CORS is properly configured, you should see the following headers in the response:

```
Access-Control-Allow-Origin
Access-Control-Allow-Methods
Access-Control-Allow-Headers
Access-Control-Allow-Credentials (if applicable)
```

If there is a CORS issue, the browser will block the request and you'll see errors like CORS policy: No 'Access-Control-Allow-Origin' header is present on the requested resource.