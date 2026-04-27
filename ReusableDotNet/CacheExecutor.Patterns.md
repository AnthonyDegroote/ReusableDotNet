# CacheExecutor et alternatives de pattern

Ce document présente `CacheExecutor<TKey, TValue>`, puis plusieurs approches alternatives avec un exemple de code pour chacune.

## 1) CacheExecutor (approche du projet)

### Présentation
`CacheExecutor<TKey, TValue>` est un cache mémoire **thread-safe**, orienté **scope d’un traitement** (pas un cache global applicatif).

Objectif principal : éviter les appels redondants à une ressource distante pendant un traitement potentiellement long.

Points clés :
- calcul unique par clé pendant le scope de vie de l’instance ;
- concurrence gérée via `ConcurrentDictionary` + `Lazy<T>` ;
- invalidation explicite avec `Invalidate(key)` après mise à jour distante.

### Noms alternatifs envisagés
- `OperationScopedCache<TKey, TValue>` (recommandé)
- `ProcessingScopeCache<TKey, TValue>`
- `DeduplicatingCache<TKey, TValue>`
- `KeyedExecutionCache<TKey, TValue>`
- `RemoteCallCache<TKey, TValue>`

### Exemple
```csharp
var cache = new CacheExecutor<string, CustomerDto>();

var customer = cache.Execute(customerId, LoadCustomer);

cache.Invalidate(customerId);

var refreshedCustomer = cache.Execute(customerId, LoadCustomer);

CustomerDto LoadCustomer(string id)
{
	return remoteApi.GetCustomer(id);
}
```

### Différences majeures vs alternatives
- scope contrôlé par l’appelant (souvent un service scoped à un traitement métier) ;
- pas de TTL/éviction automatique ;
- pas distribué nativement.

---

## 2) Memoization simple (fonction pure)

### Exemple
```csharp
public sealed class MemoizedLength
{
	private readonly Dictionary<string, int> _memo = new();

	public int Get(string value)
	{
		if (_memo.TryGetValue(value, out var cached))
		{
			return cached;
		}

		var computed = Compute(value);
		_memo[value] = computed;
		return computed;

		static int Compute(string input)
		{
			return input.Length;
		}
	}
}
```

### Différence avec `CacheExecutor`
- souvent utilisé pour des calculs purs en mémoire ;
- généralement non thread-safe sans protection supplémentaire ;
- invalidation rarement centrale.

---

## 3) Cache-Aside avec `IMemoryCache` (cache applicatif)

### Exemple
```csharp
using Microsoft.Extensions.Caching.Memory;

public sealed class ProductService(IMemoryCache memoryCache, RemoteApi remoteApi)
{
	public async Task<ProductDto> GetProductAsync(string productId, CancellationToken cancellationToken)
	{
		if (memoryCache.TryGetValue(productId, out ProductDto? cached) && cached is not null)
		{
			return cached;
		}

		var product = await remoteApi.GetProductAsync(productId, cancellationToken);

		var options = new MemoryCacheEntryOptions
		{
			AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
		};

		memoryCache.Set(productId, product, options);
		return product;
	}

	public async Task UpdateProductAsync(ProductDto product, CancellationToken cancellationToken)
	{
		await remoteApi.UpdateProductAsync(product, cancellationToken);
		memoryCache.Remove(product.Id);
	}
}
```

### Différence avec `CacheExecutor`
- cache de durée de vie applicative (pas uniquement un traitement) ;
- ajoute TTL/éviction et options de gestion mémoire ;
- très adapté au cache transversal de lecture.

---

## 4) Identity Map (unité de travail)

### Exemple
```csharp
public sealed class CustomerIdentityMap
{
	private readonly Dictionary<Guid, CustomerEntity> _tracked = new();

	public async Task<CustomerEntity> GetAsync(Guid id, Func<Guid, Task<CustomerEntity>> loader)
	{
		if (_tracked.TryGetValue(id, out var existing))
		{
			return existing;
		}

		var loaded = await loader(id);
		_tracked[id] = loaded;
		return loaded;
	}
}
```

### Différence avec `CacheExecutor`
- centré sur l’identité d’entités et leur unicité en mémoire ;
- souvent couplé à un `UnitOfWork` / ORM ;
- but principal : cohérence d’instance, pas optimisation générique de n’importe quelle clé/valeur.

---

## 5) Cache de requête HTTP (request-scoped)

### Exemple
```csharp
public sealed class RequestScopedCache(IHttpContextAccessor accessor)
{
	public TValue GetOrAdd<TValue>(string key, Func<TValue> factory)
	{
		var httpContext = accessor.HttpContext
			?? throw new InvalidOperationException("No active HttpContext");

		if (httpContext.Items.TryGetValue(key, out var existing) && existing is TValue typed)
		{
			return typed;
		}

		var created = factory();
		httpContext.Items[key] = created!;
		return created;
	}
}
```

### Différence avec `CacheExecutor`
- scope extrêmement court (une seule requête HTTP) ;
- pas destiné aux traitements hors pipeline HTTP ;
- utile pour éviter les doublons dans la même requête uniquement.

---

## Résumé pratique

Utiliser `CacheExecutor` quand :
- on a un traitement métier (potentiellement long) ;
- on veut partager un cache entre étapes de ce traitement ;
- on veut invalider explicitement après une écriture distante.

Préférer une alternative quand :
- cache global applicatif + TTL/éviction => `IMemoryCache` ;
- besoin distribué multi-instance => cache distribué (ex: Redis) ;
- besoin d’unicité d’entité dans une unité de travail => Identity Map ;
- besoin limité à une requête HTTP => request-scoped cache.
