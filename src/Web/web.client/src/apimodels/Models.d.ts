
abstract interface AbstractEFEntityWithName extends AbstractEFEntity { name: string }
abstract interface AbstractEFEntity { id: string }

interface IdTokenClaims {
  at_hash: string,
  country: string
  family_name: string,
  given_name: string
}
