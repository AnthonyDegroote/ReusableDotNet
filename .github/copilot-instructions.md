# Copilot Instructions

## Directives de projet
- Privilégier l'utilisation de fonctions locales plutôt que d'expressions lambda quand c'est possible.
- Privilégier l'utilisation d'un builder dans les tests.
- Pour les tests unitaires, utiliser [Fact] pour un seul cas de test et [Theory] pour plusieurs cas (préférer [Theory] lorsque plusieurs jeux de données sont nécessaires).
- Utiliser CacheExecutor comme cache partagé entre différentes étapes d'un même traitement (potentiellement long) pour éviter les appels redondants à des ressources distantes. Limiter son scope au traitement en cours : il ne remplace pas un cache applicatif global. Appeler Invalidate après toute mise à jour distante pour forcer la prochaine lecture à recharger la valeur distante.