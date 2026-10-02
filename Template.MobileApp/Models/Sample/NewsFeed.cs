namespace Template.MobileApp.Models.Sample;

// Top は全カテゴリの記事を集めたもの
public enum NewsCategory
{
    Top,
    Economy,
    Technology,
    Sports,
    Entertainment,
    Life
}

public sealed record NewsArticle(
    int Id,
    NewsCategory Category,
    string Title,
    string Summary,
    string Body,
    string Source,
    DateTime PublishedAt,
    bool IsBreaking,
    int CommentCount);

// 見本のダミーの記事を作る (取得の代わり)。random は 0 以上 n 未満を返す
public sealed class NewsFeed
{
    private const string Notice = "※ この記事は画面の見本のためのダミーです。実在の人物・団体・出来事とは関係ありません。";

    private static readonly Dictionary<NewsCategory, NewsSeed> Seeds = new()
    {
        [NewsCategory.Economy] = new NewsSeed(
            "うさぎ経済新聞",
            "関係者によると、今回の動きは年度の後半の業績にも影響する見通しだ。専門家は、消費の回復の度合いと海外の景気の行方を注視する必要があると指摘している。",
            [
                ("日経平均、半年ぶりの高値 ハイテク株が相場をけん引", "半導体の関連の銘柄に買いが集まり、終値は前の日から 1.8% 上がった。年末に向けた期待が広がっている。"),
                ("大手コンビニ、全店でセルフレジを導入へ", "人手不足への対応として、来年の春までに全国の店舗へ広げる。レジの待ち時間は半分になる見込み。"),
                ("円相場、1 ドル 145 円台に", "米国の金利の見通しを受けて円を買う動きが強まった。輸入品の値段への影響に関心が集まる。"),
                ("地方の空き家を使ったシェアオフィスが人気", "テレワークの広がりで利用者は 1 年で 2 倍に。地域の新しい雇用にもつながっている。"),
                ("キャッシュレス決済の比率が 5 割を超える", "小さな店舗でも導入が進み、若い世代を中心に現金を持たない人が増えている。"),
                ("少額投資の制度、口座の数が 1,000 万を突破", "積み立てで始める人が多く、20 代と 30 代の伸びが目立つ。"),
                ("物流の各社が共同配送で連携", "同じ方面の荷物をまとめて運び、トラックの積載率を上げる。運転手の負担も減らす。"),
                ("老舗の和菓子店、海外向けの通信販売を開始", "抹茶を使った焼き菓子が好調で、欧州からの注文が全体の 4 割を占める。")
            ]),
        [NewsCategory.Technology] = new NewsSeed(
            "テックうさぎ",
            "開発の担当者は「毎日の使い勝手を第一に考えた」と話す。今後はソフトウェアの更新で機能を増やし、対応する機器も順次広げる予定だという。",
            [
                ("新型スマートフォンが発表 カメラと電池の持ちが向上", "夜景の撮影の性能を高め、電池は 2 日持つ。発売は来月の中旬。"),
                ("生成 AI で議事録を自動で作成 自治体の 3 割が導入", "会議の録音から要点をまとめ、職員の作業の時間を大きく減らした。"),
                ("折りたためるノートパソコンが登場 重さは 1 kg を切る", "画面を広げると 17 インチになり、持ち運びと作業のしやすさを両立する。"),
                ("ロボット掃除機、段差を乗り越える新しい機種", "脚のような車輪で 4 cm までの段差を越え、部屋の間を自分で移動する。"),
                ("国産の小型ロケット、打ち上げに成功", "搭載した 3 基の衛星をすべて予定の軌道に入れた。次の打ち上げは来年の予定。"),
                ("電気自動車の充電の時間を半分に 新しい電池の試作に成功", "材料の組み合わせを見直し、10 分の充電で 300 km 走れるという。"),
                ("スマートウォッチで睡眠の質を採点", "心拍と体の動きから眠りの深さを推定し、毎朝の点数と助言を出す。"),
                ("オープンソースの開発者の会議が開幕 参加者は過去最多", "3 日間で 200 を超える講演があり、AI とセキュリティの話題が中心になった。")
            ]),
        [NewsCategory.Sports] = new NewsSeed(
            "うさスポーツ",
            "試合の後、選手は「チーム全員でつかんだ結果。次の試合に向けて、すぐに気持ちを切り替えたい」と話した。会場には多くのファンが詰めかけ、最後まで声援を送った。",
            [
                ("サッカー日本代表、アジア予選で快勝 若手が 2 得点", "前半の早い時間に先制し、後半も攻撃の手を緩めなかった。予選の首位を守った。"),
                ("プロ野球、首位の攻防の 3 連戦は 1 試合が雨で中止", "残りの 2 試合は 1 勝 1 敗。優勝の行方は最後の週までもつれそうだ。"),
                ("マラソン、30 km からの独走で日本記録を更新", "後半にペースを上げて後続を引き離し、記録を 12 秒縮めた。"),
                ("バスケットボール、新しいリーグの開幕戦に 1 万人", "演出にも力を入れた会場は満員になり、延長の末に地元のチームが勝った。"),
                ("テニスの国際大会、17 歳が初めての決勝へ", "準決勝でシード選手を破り、大会の最年少での決勝の進出を決めた。"),
                ("スケートボードの大会、12 歳が大技を決めて優勝", "最後の 1 本で難しい技を成功させ、逆転で頂点に立った。"),
                ("ラグビー、代表の合宿が始まる 新しい主将を発表", "来月の強化試合に向けて、若手を多く招集した。"),
                ("競泳の短水路大会、背泳ぎで大会新記録", "スタートからリードを保ち、自己ベストを 0.5 秒更新した。")
            ]),
        [NewsCategory.Entertainment] = new NewsSeed(
            "エンタメぴょん",
            "関係者は「多くの方に楽しんでもらえてうれしい」とコメントした。SNS でも感想の投稿が相次ぎ、関連する話題が一日中トレンドの上位に入った。",
            [
                ("話題のアニメ映画、公開 3 日で観客 100 万人を突破", "映像の美しさが評判を呼び、週末は多くの映画館で満席が続いた。"),
                ("人気バンドが 5 年ぶりの全国ツアーを発表", "全国の 20 都市を回る。応募が多く、追加の公演も決まった。"),
                ("連続ドラマの最終回、配信の再生数が過去最高に", "結末をめぐる考察が広がり、放送の後も視聴が伸び続けている。"),
                ("美術館で没入型の展示が始まる", "映像と音で名画の中に入ったような体験ができる。会期は来年の 1 月まで。"),
                ("ゲームの新作が世界で同時に発売 初週で 300 万本", "協力して遊ぶ仕組みが好評で、配信の視聴者数も記録を更新した。"),
                ("朝の情報番組に新しい司会者 うさうささんを起用", "親しみやすい語り口が評判で、番組は来週の月曜日から新しい体制になる。"),
                ("人気の漫画の実写化が決定 撮影は来春から", "作者も脚本の制作に加わる。出演者は年内に発表する予定。"),
                ("音楽フェスの出演者の第 1 弾を発表", "海外のアーティストも多く参加する。チケットは来月から販売する。")
            ]),
        [NewsCategory.Life] = new NewsSeed(
            "くらしのうさぎ",
            "担当者は「季節の変わり目は体調を崩しやすい。無理のない範囲で取り入れてほしい」と話している。詳しい情報は各自治体や施設のサイトで確認できる。",
            [
                ("秋の味覚のサンマが豊漁 店頭の値段は去年より 2 割安く", "脂の乗りも良く、スーパーでは特売の売り場を広げる店が増えている。"),
                ("週末は各地で紅葉が見ごろ 行楽の人出が増える見込み", "山沿いを中心に色づきが進んでいる。道路の混雑に注意が必要だ。"),
                ("インフルエンザの予防接種が始まる", "流行の前に済ませるよう、医療機関が早めの予約を呼びかけている。"),
                ("電気代を抑える冬の家電の使い方", "エアコンのフィルターの掃除と、温度の設定の見直しで 1 割ほど節約できる。"),
                ("駅前に図書館とカフェが一体の施設がオープン", "飲み物を片手に本を選べ、夜は 10 時まで利用できる。"),
                ("家庭の備蓄を見直す 防災の目安は 1 週間分", "水と食料に加え、常備薬や携帯トイレの準備も勧めている。"),
                ("ペットと泊まれる宿が増加 予約は前の年の 1.5 倍", "専用の食事や散歩の道を用意する宿が人気を集めている。"),
                ("朝ごはんを食べる人ほど睡眠の満足度が高い", "1 万人の調査で、生活のリズムと眠りの関係が改めて示された。")
            ])
    };

    private readonly Func<int, int> random;

    // カテゴリごとの、これから使う見出しの順
    private readonly Dictionary<NewsCategory, Queue<(NewsCategory Category, int Index)>> headlines =
        Enum.GetValues<NewsCategory>().ToDictionary(static x => x, static _ => new Queue<(NewsCategory Category, int Index)>());

    private int nextId;

    public NewsFeed(Func<int, int> random)
    {
        this.random = random;
    }

    // 新しい順。時刻は now から少しずつさかのぼる
    public IReadOnlyList<NewsArticle> Create(NewsCategory category, DateTime now, int count) =>
        CreateArticles(Enumerable.Range(0, count).Select(_ => NextHeadline(category)).ToArray(), now);

    // 今の時刻の記事
    public NewsArticle CreateLatest(NewsCategory category, DateTime now)
    {
        var (actual, index) = NextHeadline(category);
        return CreateArticle(actual, index, now);
    }

    // 速報 (数分前の記事)
    public NewsArticle CreateBreaking(DateTime now)
    {
        var category = (NewsCategory)(1 + random(Seeds.Count));
        return CreateArticle(category, random(Seeds[category].Headlines.Count), now.AddMinutes(-(1 + random(10))), true);
    }

    // 同じカテゴリの別の記事 (一覧の見出しの順は進めない)
    public IReadOnlyList<NewsArticle> CreateRelated(NewsArticle article, int count) =>
        CreateArticles(
            Shuffle(article.Category)
                .Where(x => Seeds[x.Category].Headlines[x.Index].Title != article.Title)
                .Take(count)
                .ToArray(),
            article.PublishedAt);

    // 見出しはカテゴリごとに混ぜた順に使い切ってから繰り返す。
    // 周の変わり目で同じ見出しが続かないように、残りが 1 つのときに次の周を足し、その先頭が残りと同じなら最後と入れ替える
    private (NewsCategory Category, int Index) NextHeadline(NewsCategory category)
    {
        var queue = headlines[category];
        if (queue.Count <= 1)
        {
            var next = Shuffle(category);
            if ((queue.Count == 1) && (next[0] == queue.Peek()))
            {
                (next[0], next[^1]) = (next[^1], next[0]);
            }

            foreach (var headline in next)
            {
                queue.Enqueue(headline);
            }
        }

        return queue.Dequeue();
    }

    // Top は各カテゴリから集める
    private (NewsCategory Category, int Index)[] Shuffle(NewsCategory category)
    {
        var candidates = Seeds
            .Where(x => (category == NewsCategory.Top) || (x.Key == category))
            .SelectMany(static x => Enumerable.Range(0, x.Value.Headlines.Count).Select(i => (Category: x.Key, Index: i)))
            .ToArray();
        for (var i = candidates.Length - 1; i > 0; i--)
        {
            var j = random(i + 1);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }

        return candidates;
    }

    private List<NewsArticle> CreateArticles((NewsCategory Category, int Index)[] source, DateTime now)
    {
        var articles = new List<NewsArticle>(source.Length);
        var time = now;
        foreach (var (category, index) in source)
        {
            time = time.AddMinutes(-(5 + random(90)));
            articles.Add(CreateArticle(category, index, time));
        }

        return articles;
    }

    // 速報はまれに、コメントの数は少ない記事が多い
    private NewsArticle CreateArticle(NewsCategory category, int index, DateTime time, bool? breaking = null)
    {
        var seed = Seeds[category];
        var (title, summary) = seed.Headlines[index];
        var comments = random(4) == 0 ? 100 + random(900) : random(60);
        nextId++;
        return new NewsArticle(
            nextId,
            category,
            title,
            summary,
            $"{summary}\n\n{seed.Paragraph}\n\n{Notice}",
            seed.Source,
            time,
            breaking ?? (random(8) == 0),
            comments);
    }

    private sealed record NewsSeed(string Source, string Paragraph, IReadOnlyList<(string Title, string Summary)> Headlines);
}
