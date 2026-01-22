"""seed company data

Revision ID: 2026012202
Revises: 2026012201
Create Date: 2026-01-22 00:00:00.000000
"""

from alembic import op

revision = "2026012202"
down_revision = "2026012201"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.execute(
        """
        INSERT INTO company (name, industry, email, phone, created_utc, updated_utc) VALUES
        ('Long Ranch Holdings', 'Agriculture', 'contact@longranch.com', '555-2001', NOW(), NOW()),
        ('Trailhead Logistics', 'Transportation', 'info@trailheadlogistics.com', '555-2002', NOW(), NOW()),
        ('Sunset Outfitters', 'Retail', 'support@sunsetoutfitters.com', '555-2003', NOW(), NOW());
        """
    )


def downgrade() -> None:
    op.execute("DELETE FROM company;")
