# Copilot Instructions

## Directives de projet
- Privilégier l'utilisation de fonctions locales plutôt que d'expressions lambda quand c'est possible.
- Privilégier l'utilisation d'un builder dans les tests.
- Pour les tests unitaires, utiliser [Fact] pour un seul cas de test et [Theory] pour plusieurs cas (préférer [Theory] lorsque plusieurs jeux de données sont nécessaires).