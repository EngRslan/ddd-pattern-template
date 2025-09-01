using Engrslan.Types;
using NJsonSchema;
using NJsonSchema.Generation;

namespace Engrslan.Swagger;

public class DefaultSchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        if (
            context.ContextualType != typeof(EncryptedInt) && 
            context.ContextualType != typeof(EncryptedLong)) return;
        context.Schema.Type = JsonObjectType.String;
        context.Schema.Format = null; // optionally set format or leave null
    }
}