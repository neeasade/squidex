// ==========================================================================
//  Field.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;
// ReSharper disable InvertIf
// ReSharper disable ConvertIfStatementToReturnStatement

namespace Squidex.Core.Schemas
{
    public abstract class Field : Cloneable
    {
        private readonly Lazy<List<IValidator>> validators;
        private readonly long id;
        private string name;
        private bool isDisabled;
        private bool isHidden;

        public long Id
        {
            get { return id; }
        }

        public string Name
        {
            get { return name; }
        }

        public bool IsHidden
        {
            get { return isHidden; }
        }

        public bool IsDisabled
        {
            get { return isDisabled; }
        }

        public abstract FieldProperties RawProperties { get; }

        protected Field(long id, string name)
        {
            Guard.ValidPropertyName(name, nameof(name));
            Guard.GreaterThan(id, 0, nameof(id));

            this.id = id;

            this.name = name;

            validators = new Lazy<List<IValidator>>(() => new List<IValidator>(CreateValidators()));
        }

        public abstract Field Update(FieldProperties newProperties);

        public async Task ValidateAsync(JToken value, ICollection<string> errors, Language language = null)
        {
            Guard.NotNull(value, nameof(value));

            var rawErrors = new List<string>();
            try
            {
                var typedValue = value.Type == JTokenType.Null ? null :  ConvertValue(value);

                foreach (var validator in validators.Value)
                {
                    await validator.ValidateAsync(typedValue, rawErrors);
                }
            }
            catch
            {
                rawErrors.Add("<FIELD> is not a valid value");
            }

            if (rawErrors.Count > 0)
            {
                var displayName = !string.IsNullOrWhiteSpace(RawProperties.Label) ? RawProperties.Label : name;

                if (language != null)
                {
                    displayName += $" ({language.Iso2Code})";
                }

                foreach (var error in rawErrors)
                {
                    errors.Add(error.Replace("<FIELD>", displayName));
                }
            }
        }

        public Field Hide()
        {
            return Clone<Field>(clone => clone.isHidden = true);
        }

        public Field Show()
        {
            return Clone<Field>(clone => clone.isHidden = false);
        }

        public Field Disable()
        {
            return Clone<Field>(clone => clone.isDisabled = true);
        }

        public Field Enable()
        {
            return Clone<Field>(clone => clone.isDisabled = false);
        }

        public Field Rename(string newName)
        {
            if (!newName.IsSlug())
            {
                var error = new ValidationError("Name must be a valid slug", "Name");

                throw new ValidationException($"Cannot rename the field '{name}' ({id})", error);
            }

            return Clone<Field>(clone => clone.name = newName);
        }

        protected abstract IEnumerable<IValidator> CreateValidators();

        protected abstract object ConvertValue(JToken value);
    }
}