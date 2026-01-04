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





1️⃣ 显示「命中的字段」（Badge：Email / 名字 / Guid）

❓为什么要做这个？



用户搜索 jack 时，可能命中：



FirstName = Jackson



LastName = Jackman



Email = jack@mail.com



Guid = jack-xxxx



👉 用户不知道是哪一列命中的，会很困惑



优秀 UX：要告诉用户“为什么这个结果会出现”



✅ 正确做法（核心思想）

❌ 不要在 UI 自己判断

✅ 让 SQL 告诉你：是哪一列匹配的

🧠 SQL 思路（不是代码细节）



在 SQL 里加一个 计算列：



CASE

&nbsp;   WHEN FirstName LIKE @kw + '%' THEN 'FirstName'

&nbsp;   WHEN LastName  LIKE @kw + '%' THEN 'LastName'

&nbsp;   WHEN Email     LIKE @kw + '%' THEN 'Email'

&nbsp;   WHEN GuidText  LIKE @kw + '%' THEN 'Guid'

END AS MatchType





👉 每一行都会多一个字段：

MatchType = "Email" / "FirstName" / ...



🎨 UI 表现（MudBlazor）



在 ItemTemplate 里：



左边：用户信息



右边：一个 MudChip



Kelly Aaliyah        \[LastName]

aaliyah@mail.com     \[Email]





👉 用户一眼就懂



2️⃣ 给结果打「相关度分数」（Scoring）

❓为什么要打分？



不是所有匹配都一样重要：



搜索	最想要

jack	FirstName = Jack

kel	LastName = Kelly

@gm	Email

f3a	Guid



👉 排序不能只靠字母顺序



✅ 正确设计方式

在 SQL 里算分数（不是在 C#）

完全匹配       = 100 分

前缀匹配       = 90 分

Email 前缀     = 80 分

Guid 前缀      = 70 分



🧠 好处



SQL 一次完成



排序稳定



UI 不用管业务逻辑



以后可以微调权重



🎯 最终排序逻辑

ORDER BY Score DESC, FirstName





👉 最相关的永远排在最前



https://zetbit.tech/categories/asp-dot-net-core/39/display-live-data-from-database-in-blazor-sqltabledependency-and-signalr

