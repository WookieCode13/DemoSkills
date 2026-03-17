from collections.abc import Callable
from typing import Any

from fastapi import Depends, HTTPException, status
from sqlalchemy.orm import Session

from .auth_context import UserAuthContext, get_user_auth_context
from .principal import Principal


def require_auth_context(
    db: Session,
    principal: Principal,
) -> UserAuthContext:
    auth_context = get_user_auth_context(db, principal)
    if auth_context is None:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Authenticated user is not provisioned for application access.",
        )
    return auth_context


def build_auth_context_dependency(
    get_db_dependency: Callable[..., Any],
    get_principal_dependency: Callable[..., Any],
) -> Callable[..., UserAuthContext]:
    def dependency(
        db: Session = Depends(get_db_dependency),
        principal: Principal = Depends(get_principal_dependency),
    ) -> UserAuthContext:
        return require_auth_context(db, principal)

    return dependency


def build_require_permission(
    permission_code: str,
    get_db_dependency: Callable[..., Any],
    get_principal_dependency: Callable[..., Any],
) -> Callable[..., UserAuthContext]:
    auth_context_dependency = build_auth_context_dependency(
        get_db_dependency=get_db_dependency,
        get_principal_dependency=get_principal_dependency,
    )

    def dependency(
        auth_context: UserAuthContext = Depends(auth_context_dependency),
    ) -> UserAuthContext:
        if permission_code not in auth_context.permissions:
            raise HTTPException(
                status_code=status.HTTP_403_FORBIDDEN,
                detail=f"Missing required permission: {permission_code}",
            )
        return auth_context

    return dependency
