# API Overview

This document summarizes the main backend modules and their purpose.

## Auth

Handles user authentication, profile management and password recovery.

Main endpoints:

- `POST /auth/login`
- `GET /auth/users/{id}`
- `PATCH /auth/users/{id}`
- `DELETE /auth/users/{id}`

## Categories

Handles the available food categories used by huariques.

Main endpoints:

- `GET /categories`
- `POST /categories`

## Huariques

Handles huarique registration, listing, search, update and image upload.

Main endpoints:

- `GET /huariques`
- `POST /huariques`
- `PATCH /huariques/{id}`
- `DELETE /huariques/{id}`

## Favorites

Handles favorite huariques per user.

Main endpoints:

- `GET /favorites`
- `POST /favorites/{huariqueId}`
- `DELETE /favorites/{huariqueId}`

## Reviews

Handles user reviews and ratings for huariques.

Main endpoints:

- `GET /reviews`
- `POST /reviews`

## Promotions

Handles promotional offers created by huarique owners.

Main endpoints:

- `GET /promos`
- `POST /promos`
- `PATCH /promos/{id}`
- `DELETE /promos/{id}`

## Memberships

Handles available plans and user subscriptions.

Main endpoints:

- `GET /plans`
- `GET /subscriptions`
- `POST /subscriptions`
