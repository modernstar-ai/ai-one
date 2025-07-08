# Archiecture

## Front End Application
The front end application is a React app that is built with Vite.

It could be deployed as a static web app, but due to many customer requirements for all resources to be deployed in VNETs it is being deployed onto a web app. 

It also could have been deployed onto the same web app as the API, but a deliberate decision has been made to ensure that APIM can easily be inserted between the front end and the back end. 