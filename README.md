🎯 Objectif



Recherche par préfixe dans :



FirstName



LastName



Email



Guid (texte)



Exemples :



jac → Jackson, Jacob, jacob@mail.com



doe → John Doe



abc@ → abc@domain.com



f3a2 → GUID partiel



⚠️ Règle d’or (très importante)



❌ PAS de %keyword%

✅ UNIQUEMENT keyword%



Sinon :



aucun index utilisé



table scan



autocomplete lent



✅ Requête SQL RECOMMANDÉE (autocomplete-safe)

SELECT TOP (20)

&nbsp;      Id,

&nbsp;      FirstName,

&nbsp;      LastName,

&nbsp;      Email,

&nbsp;      Guid

FROM dbo.\[User]

WHERE

&nbsp;     FirstName COLLATE Latin1\_General\_CI\_AI LIKE @kw + '%'

&nbsp;  OR LastName  COLLATE Latin1\_General\_CI\_AI LIKE @kw + '%'

&nbsp;  OR Email     COLLATE Latin1\_General\_CI\_AI LIKE @kw + '%'

&nbsp;  OR CONVERT(varchar(36), Guid) LIKE @kw + '%'

ORDER BY

&nbsp;   CASE

&nbsp;       WHEN FirstName LIKE @kw + '%' THEN 1

&nbsp;       WHEN LastName  LIKE @kw + '%' THEN 2

&nbsp;       WHEN Email     LIKE @kw + '%' THEN 3

&nbsp;       WHEN CONVERT(varchar(36), Guid) LIKE @kw + '%' THEN 4

&nbsp;       ELSE 5

&nbsp;   END,

&nbsp;   FirstName;



Pourquoi ce ORDER BY CASE ?



👉 Résultats plus intelligents :



prénom



nom



email



guid



UX ++



⚠️ IMPORTANT : limitation GUID

CONVERT(varchar(36), Guid)





✔️ OK pour autocomplete

❌ non indexable



👉 Guid = fallback, pas critère principal



🚀 Index RECOMMANDÉS (obligatoires)

🔹 Index principal (autocomplete humain)

CREATE NONCLUSTERED INDEX User\_Name\_Autocomplete\_IDX

ON dbo.\[User] (FirstName, LastName)

INCLUDE (Id, Email)

WITH (FILLFACTOR = 90);





✔️ couvre :



FirstName



LastName



ORDER BY partiel



🔹 Index Email

CREATE NONCLUSTERED INDEX User\_Email\_IDX

ON dbo.\[User] (Email)

INCLUDE (Id, FirstName, LastName)

WITH (FILLFACTOR = 90);



🔹 Guid

CREATE NONCLUSTERED INDEX User\_Guid\_IDX

ON dbo.\[User] (Guid);





👉 utilisé seulement si recherche exacte

👉 le LIKE sur Guid ne profitera pas de l’index



🧠 Alternative PRO (encore meilleure)

➜ Ajouter une colonne calculée

1️⃣ Colonne calculée persistée

ALTER TABLE dbo.\[User]

ADD GuidText AS CONVERT(varchar(36), Guid) PERSISTED;



2️⃣ Index dessus

CREATE NONCLUSTERED INDEX User\_GuidText\_IDX

ON dbo.\[User] (GuidText);



3️⃣ Requête améliorée

OR GuidText LIKE @kw + '%'





👉 maintenant indexable 🚀



🧩 Implémentation C# (Repository)

public async IAsyncEnumerable<User> StreamUsersAsync(

&nbsp;   string keyword,

&nbsp;   \[EnumeratorCancellation] CancellationToken ct)

{

&nbsp;   await using var conn =

&nbsp;       \_connectionFactory.CreateConnection() as SqlConnection;



&nbsp;   await conn.OpenAsync(ct);



&nbsp;   await using var reader = await conn.ExecuteReaderAsync(

&nbsp;       new CommandDefinition(

&nbsp;           """

&nbsp;           SELECT TOP (20)

&nbsp;                  Id, FirstName, LastName, Email, Guid

&nbsp;           FROM dbo.\[User]

&nbsp;           WHERE

&nbsp;                 FirstName COLLATE Latin1\_General\_CI\_AI LIKE @kw + '%'

&nbsp;              OR LastName  COLLATE Latin1\_General\_CI\_AI LIKE @kw + '%'

&nbsp;              OR Email     COLLATE Latin1\_General\_CI\_AI LIKE @kw + '%'

&nbsp;              OR GuidText  LIKE @kw + '%'

&nbsp;           ORDER BY

&nbsp;               CASE

&nbsp;                   WHEN FirstName LIKE @kw + '%' THEN 1

&nbsp;                   WHEN LastName  LIKE @kw + '%' THEN 2

&nbsp;                   WHEN Email     LIKE @kw + '%' THEN 3

&nbsp;                   WHEN GuidText  LIKE @kw + '%' THEN 4

&nbsp;                   ELSE 5

&nbsp;               END,

&nbsp;               FirstName;

&nbsp;           """,

&nbsp;           new { kw = keyword },

&nbsp;           cancellationToken: ct));



&nbsp;   var parser = reader.GetRowParser<User>();



&nbsp;   while (await reader.ReadAsync(ct))

&nbsp;   {

&nbsp;       yield return parser(reader);

&nbsp;   }

}



🎯 UX CONSEILS (très importants)



✔️ MinCharacters = 2 (ou 3)

✔️ TOP (20) max

✔️ spinner immédiat

✔️ debounce ≥ 300 ms

❌ pas de %keyword%

❌ pas de recherche full-text ici

