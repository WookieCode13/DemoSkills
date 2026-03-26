from dataclasses import dataclass, field

from sqlalchemy import text
from sqlalchemy.orm import Session

from .principal import Principal


@dataclass
class UserAuthContext:
    app_user_id: str
    cognito_sub: str
    email: str | None = None
    base_role_level: int | None = None
    global_role_code: str | None = None
    permissions: list[str] = field(default_factory=list)


def get_user_auth_context(db: Session, principal: Principal) -> UserAuthContext | None:
    user_row = db.execute(
        text(
            """
            select
                app_user_id,
                cognito_sub,
                email,
                base_role_level,
                global_role_code
            from _auth.app_user
            where cognito_sub = :cognito_sub
              and is_active = true
            limit 1
            """
        ),
        {"cognito_sub": principal.sub},
    ).mappings().first()

    if user_row is None:
        return None

    permissions: list[str] = []
    global_role_code = user_row["global_role_code"]
    if global_role_code:
        permission_rows = db.execute(
            text(
                """
                select p.permission_code
                from _auth.role_permission rp
                join _auth.permission p
                  on p.permission_code = rp.permission_code
                where rp.role_code = :role_code
                  and p.is_active = true
                """
            ),
            {"role_code": global_role_code},
        ).scalars().all()
        permissions = [permission_code for permission_code in permission_rows if permission_code]

    return UserAuthContext(
        app_user_id=str(user_row["app_user_id"]),
        cognito_sub=str(user_row["cognito_sub"]),
        email=user_row["email"],
        base_role_level=user_row["base_role_level"],
        global_role_code=global_role_code,
        permissions=permissions,
    )
