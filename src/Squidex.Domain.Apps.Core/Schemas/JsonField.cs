﻿// ==========================================================================
//  JsonField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using Squidex.Domain.Apps.Core.Schemas.Validators;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class JsonField : Field<JsonFieldProperties>
    {
        public JsonField(long id, string name, Partitioning partitioning)
            : this(id, name, partitioning, new JsonFieldProperties())
        {
        }

        public JsonField(long id, string name, Partitioning partitioning, JsonFieldProperties properties)
            : base(id, name, partitioning, properties)
        {
        }

        protected override IEnumerable<IValidator> CreateValidators()
        {
            if (Properties.IsRequired)
            {
                yield return new RequiredValidator();
            }
        }

        public override object ConvertValue(JToken value)
        {
            return value;
        }

        protected override void PrepareJsonSchema(JsonProperty jsonProperty, Func<string, JsonSchema4, JsonSchema4> schemaResolver)
        {
            jsonProperty.Type = JsonObjectType.Object;
        }

        protected override IEdmTypeReference CreateEdmType()
        {
            return null;
        }
    }
}
