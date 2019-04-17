# Microsoft Dynamics 365 Fraud Protection - Sample merchant application
The Microsoft Dynamics 365 Fraud Protection sample merchant application demonstrates how to call the [Dynamics 365 Fraud Protection APIs](https://apidocs.microsoft.com/services/dynamics365fraudprotection) in the context of an online merchant who sells clothing and other goods. This sample may be useful if you are in the process of integrating with the Dynamics 365 Fraud Protection APIs, or if you want to see how new API endpoints/features are integrated when there are new API versions.

It demonstrates two primary operations using the Dynamics 365 Fraud Protection APIs:
- Setting up the user and their associated information in the Dynamics 365 Fraud Protection system (for example, basic info, payment methods, addresses, and so on).
- Requesting a Dynamics 365 Fraud Protection recommendation for a purchase and using that recommendation to decide whether to charge the customer.

## Contents
There are two main sections to the sample application:
- [API documentation](./docs) explaining how to authenticate with and call the APIs. Plus, API usage guidance is given for various scenarios like guest checkout.
- [Sample application](./src) that you can learn from and run yourself.

Much of the code is based on the [eShopOnWeb](https://github.com/dotnet-architecture/eShopOnWeb) application.

## Product documentation
In addition to this API documentation, you can read the complimentary [product documentation for Dynamics 365 Fraud Protection](https://go.microsoft.com/fwlink/?linkid=2082391). It covers the broad set of Dynamics 365 Fraud Protection features such as the rules engine, reporting, and customer support. It also contains release notes and highlights planned, upcoming features.

## Privacy and telemetry

Once properly configured, this sample site uses Microsoft device fingerprinting to send device telemetry to Microsoft for the purposes of demonstrating Dynamics 365 Fraud Protection. To disable device fingerprinting, remove code related to it rather than configuring it. 

## Microsoft Open Source code of conduct

For additional information, see the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct).
