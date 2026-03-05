import json
import os
from dataclasses import dataclass
from functools import lru_cache
from urllib.request import urlopen

from .constants import (
    CLAIM_CLIENT_ID,
    CLAIM_EMAIL,
    CLAIM_GROUPS,
    CLAIM_SUB,
    CLAIM_TOKEN_USE,
    JWT_AUTHORITY_ENV,
    JWT_CLIENT_ID_ENV,
)
from .principal import Principal


@dataclass(frozen=True)
class JwtSettings:
    authority: str
    client_id: str


def load_jwt_settings_from_env() -> JwtSettings:
    authority = os.getenv(JWT_AUTHORITY_ENV) or os.getenv("JWT_AUTHORITY")
    client_id = os.getenv(JWT_CLIENT_ID_ENV) or os.getenv("JWT_CLIENT_ID")
    if not authority:
        raise RuntimeError(f"Missing required environment variable: {JWT_AUTHORITY_ENV}")
    if not client_id:
        raise RuntimeError(f"Missing required environment variable: {JWT_CLIENT_ID_ENV}")
    return JwtSettings(authority=authority.rstrip("/"), client_id=client_id)


@lru_cache(maxsize=8)
def _get_jwks(authority: str) -> dict:
    jwks_url = f"{authority}/.well-known/jwks.json"
    with urlopen(jwks_url, timeout=10) as response:
        return json.loads(response.read().decode("utf-8"))


def _get_signing_key(token: str, authority: str):
    import jwt

    unverified_header = jwt.get_unverified_header(token)
    kid = unverified_header.get("kid")
    if not kid:
        raise ValueError("JWT header missing kid.")

    jwks = _get_jwks(authority)
    for key in jwks.get("keys", []):
        if key.get("kid") == kid:
            return jwt.algorithms.RSAAlgorithm.from_jwk(json.dumps(key))
    raise ValueError("No matching JWK key found for token kid.")


def validate_access_token(token: str, settings: JwtSettings) -> Principal:
    import jwt
    from jwt import InvalidTokenError

    key = _get_signing_key(token, settings.authority)
    try:
        claims = jwt.decode(
            token,
            key=key,
            algorithms=["RS256"],
            issuer=settings.authority,
            options={"verify_aud": False},
        )
    except InvalidTokenError as ex:
        raise ValueError(f"JWT validation failed: {ex}") from ex

    token_use = claims.get(CLAIM_TOKEN_USE)
    client_id = claims.get(CLAIM_CLIENT_ID)
    if token_use != "access":
        raise ValueError("Invalid token_use; expected access token.")
    if client_id != settings.client_id:
        raise ValueError("Invalid client_id for this API.")

    groups = claims.get(CLAIM_GROUPS) or []
    if not isinstance(groups, list):
        groups = []

    sub = claims.get(CLAIM_SUB)
    if not isinstance(sub, str) or not sub:
        raise ValueError("Token missing required sub claim.")

    email = claims.get(CLAIM_EMAIL)
    email_value = email if isinstance(email, str) else None

    return Principal(sub=sub, email=email_value, groups=groups, claims=claims)
