﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Text.Json.Serialization;

namespace Contoso.FraudProtection.ApplicationCore.Entities.FraudProtectionApiModels.AccountProtection
{
    public class Address
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AddressType AddressType { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        public string Street1 { get; set; }

        public string Street2 { get; set; }

        public string Street3 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string District { get; set; }

        public string ZipCode { get; set; }

        public string CountryRegion { get; set; }
    }

    public enum AddressType
    {
        Primary,
        Billing,
        Shipping,
        Alternative
    }
}
