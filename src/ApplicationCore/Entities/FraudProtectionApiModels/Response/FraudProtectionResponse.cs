// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Contoso.FraudProtection.ApplicationCore.Entities.FraudProtectionApiModels
{
    public class FraudProtectionResponse
    {
        public Dictionary<string, object> ResultDetails { get; set; }
    }
}
