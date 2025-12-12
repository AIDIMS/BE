-- Add migration for Notification entity updates
-- This adds NotificationType enum and RelatedVisitId field

-- Add Type column
ALTER TABLE "Notifications"
ADD COLUMN "Type" integer NOT NULL DEFAULT 4; -- General = 4

-- Add RelatedVisitId column
ALTER TABLE "Notifications"
ADD COLUMN "RelatedVisitId" uuid NULL;

-- Add foreign key constraint
ALTER TABLE "Notifications"
ADD CONSTRAINT "FK_Notifications_PatientVisits_RelatedVisitId"
FOREIGN KEY ("RelatedVisitId") REFERENCES "PatientVisits"("Id")
ON DELETE SET NULL;

-- Create index for better query performance
CREATE INDEX "IX_Notifications_RelatedVisitId" ON "Notifications"("RelatedVisitId");
CREATE INDEX "IX_Notifications_UserId_IsRead" ON "Notifications"("UserId", "IsRead");
CREATE INDEX "IX_Notifications_Type" ON "Notifications"("Type");
