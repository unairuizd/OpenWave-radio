# Spec Delta

## REMOVED Requirements

### Requirement: Favoritos placeholder

**Reason**: `Favoritos` now lists, plays and removes the logged-in user's stored favourites, and shows a login invitation when no user is logged in.

**Migration**: The behaviour of `Favoritos` is defined by the `favorites-browsing` capability; the empty-state text `Aún no tienes emisoras favoritas.` is kept there for logged-in users without favourites.
