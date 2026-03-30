using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSure.ClaimsService.Migrations
{
    /// <inheritdoc />
    public partial class SyncMissingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent schema fix: ensuring columns exist in case of database drift
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Claims') AND name = 'ApprovedAmount')
                    ALTER TABLE [Claims] ADD [ApprovedAmount] decimal(18,2) NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Claims') AND name = 'CreatedAt')
                    ALTER TABLE [Claims] ADD [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE();
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Claims') AND name = 'RejectionReason')
                    ALTER TABLE [Claims] ADD [RejectionReason] nvarchar(max) NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Claims') AND name = 'UpdatedAt')
                    ALTER TABLE [Claims] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE();
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ClaimDocuments') AND name = 'ContentType')
                    ALTER TABLE [ClaimDocuments] ADD [ContentType] nvarchar(100) NOT NULL DEFAULT '';
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ClaimDocuments') AND name = 'FileSize')
                    ALTER TABLE [ClaimDocuments] ADD [FileSize] bigint NOT NULL DEFAULT 0;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ClaimDocuments') AND name = 'UploadedAt')
                    ALTER TABLE [ClaimDocuments] ADD [UploadedAt] datetime2 NOT NULL DEFAULT GETUTCDATE();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
