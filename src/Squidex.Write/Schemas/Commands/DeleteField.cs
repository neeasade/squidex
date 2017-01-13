// ==========================================================================
//  DeleteField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Write.Schemas.Commands
{
    public class DeleteField : SchemaAggregateCommand
    {
        public long FieldId { get; set; }
    }
}