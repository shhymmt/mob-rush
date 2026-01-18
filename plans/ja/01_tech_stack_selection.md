# 技術スタック選定

## 背景

Mob Rushを構築するためのゲームフレームワークを選ぶ必要がある。TypeScriptとnpmには慣れているが、ゲーム専用のライブラリやエンジンは使ったことがない。

重要な問い：使い慣れたWebベースのフレームワークを使うべきか、プロが使うUnityのような本格的なゲームエンジンを使うべきか？

---

## 要件

技術スタックに求めること：

| 要件 | 理由 |
|------|------|
| 100体以上の移動オブジェクト処理 | コアゲームプレイに多数のエンティティが関わる |
| モバイルで高パフォーマンス | ターゲットプラットフォームはスマートフォン |
| 活発なコミュニティ | 詰まった時に答えを見つけられる必要がある |
| AI（Claude）フレンドリー | Claudeが効果的にコードを書ける必要がある |
| アプリストアへの道筋 | 最終的にiOS/Androidで公開したい |

---

## 検討したオプション

### オプションA: 素のCanvas API

**メリット:**
- 依存関係なし、最小バンドルサイズ
- すべてを完全にコントロール
- ブラウザネイティブ

**デメリット:**
- すべてをゼロから構築する必要がある
- スプライト処理、アニメーション、衝突検出の組み込みなし
- 多数オブジェクトでのパフォーマンス上限

**結論**: 作業量が多すぎる、100体以上のユニットでパフォーマンス懸念

---

### オプションB: PixiJS

**メリット:**
- 優れた2Dレンダリングパフォーマンス（WebGL）
- 軽量、レンダリングに特化
- TypeScriptサポート

**デメリット:**
- レンダリングのみ - ゲームロジック、物理なし
- 衝突、サウンド用に追加ライブラリが必要
- それでもブラウザベース（モバイルパフォーマンス限界）

**結論**: 良いレンダラーだが、ゲーム機能が不足しすぎ

---

### オプションC: Phaser 3

**メリット:**
- フルゲームフレームワーク（物理、入力、オーディオ、シーン）
- 巨大なコミュニティ、大量のサンプル
- 組み込みのアーケード物理
- TypeScriptサポート
- 馴染みのあるWeb開発ワークフロー（npm、Vite）

**デメリット:**
- ブラウザベース = モバイルでのパフォーマンス上限
- バンドルサイズ（〜1MB）
- Mob Controlと同じエンジンではない（参考にしにくい）
- Web→モバイルラッパー（Capacitor）は複雑さを追加

**結論**: Webゲームには良いが、モバイルファーストのハイパーカジュアルには不向き

---

### オプションD: Unity（推奨）

**メリット:**
- **Mob Controlと同じエンジン** - オリジナルの動作を直接参照できる
- ネイティブモバイルパフォーマンス（iOS/Androidにコンパイル）
- 1000体以上のオブジェクトを楽々処理
- 物理、パーティクル、オーディオ、UIが組み込み
- 巨大なコミュニティ、無数のチュートリアル
- App Store / Google Playへの直接的な道筋
- Claude CodeはC#を効果的に書ける

**デメリット:**
- Unity Editorのインストールが必要（数GB）
- TypeScriptではなくC#
- シーン編集にはUnity GUIが必要
- 重いプロジェクト構造（.metaファイルなど）

**結論**: モバイルハイパーカジュアルゲームに最適

---

### オプションE: Godot

**メリット:**
- 軽量、オープンソース
- GDScriptは初心者フレンドリー
- 良い2Dサポート

**デメリット:**
- Unityより小さいコミュニティ
- Unity/C#に比べてClaudeの習熟度が低い
- モバイルゲームの参考資料が少ない

**結論**: 良いエンジンだが、このジャンルではUnityの方がリソースが多い

---

## 決定：なぜUnityか？

### 気づき

最初はTypeScript（自分のコンフォートゾーン）を使うPhaserに傾いていた。しかし考え直した：

1. **Mob ControlはUnityで作られた** - すべてのチュートリアル、解説、クローンガイドがUnityを参照
2. **Claude Codeがコードを書く** - AI が実装を担当するならC#の壁はずっと低い
3. **モバイルが目標** - ハイパーカジュアルゲームはモバイルで生きる、ブラウザではない
4. **パフォーマンスが重要** - エフェクト付きで100体以上のユニットにはネイティブパフォーマンスが必要

### 考えを変えたもの

| 懸念 | 解決策 |
|------|--------|
| 「C#を知らない」 | Claudeがコードを書く、私はレビューして学ぶ |
| 「Unityは複雑」 | 2Dゲームでは機能の小さなサブセットだけ使う |
| 「セットアップが重い」 | 一度きりのインストール、その後は簡単なワークフロー |
| 「ブラウザでプレビューできない」 | Unity Editorに即座のプレイモードがある |

---

## 最終決定

### ゲームエンジン: Unity 6 LTS

```
┌─────────────────────────────────────────┐
│              Unity 6 LTS                │
├─────────────────────────────────────────┤
│  レンダリング │  Universal Render Pipeline (2D) │
│  物理演算    │  Unity Physics 2D        │
│  入力        │  New Input System        │
│  オーディオ  │  Built-in Audio          │
│  UI          │  Unity UI / UI Toolkit   │
│  パーティクル │  Particle System         │
│  プラットフォーム │  iOS, Android, WebGL │
└─────────────────────────────────────────┘
```

Mob Rushに特にUnityが適している理由：

| 必要なこと | Unity機能 |
|-----------|----------|
| 多数のユニット生成 | GameObject.Instantiateでオブジェクトプーリング |
| ユニットが上に移動 | Rigidbody2Dの速度 |
| ゲートとの衝突 | Collider2Dトリガー |
| タッチ/クリック入力 | Input System（ポインターサポート） |
| 複数ステージ | シーン管理 |
| パーティクルとエフェクト | Particle System |
| モバイルデプロイ | iOS/Androidに直接ビルド |

---

### 言語: C#

- 強い型付け、TypeScriptに似た構造
- Claudeは信頼性の高いC#コードを生成
- 優れたUnity統合
- 巨大なサンプルエコシステム

---

### バージョン管理

Unity固有の考慮事項：
- Library/、Temp/、Logs/に`.gitignore`を使用
- .metaファイルは追跡（Unity必須）
- テキストベースのアセットシリアライゼーションを使用

---

## プロジェクト構造

```
samples/mob-rush/
├── docs/
│   └── plans/                    # 計画ドキュメント
├── Assets/
│   ├── Scenes/
│   │   ├── BootScene.unity       # ローディング画面
│   │   ├── MenuScene.unity       # メインメニュー
│   │   └── GameScene.unity       # メインゲームプレイ
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameManager.cs    # ゲーム状態管理
│   │   │   └── ObjectPool.cs     # オブジェクトプーリングユーティリティ
│   │   ├── Entities/
│   │   │   ├── Unit.cs           # プレイヤーユニット
│   │   │   ├── Gate.cs           # 乗算ゲート
│   │   │   └── EnemyBase.cs      # 破壊するターゲット
│   │   ├── Systems/
│   │   │   ├── SpawnSystem.cs    # ユニット生成
│   │   │   └── StageManager.cs   # ステージ進行
│   │   └── UI/
│   │       └── GameUI.cs         # HUD要素
│   ├── Prefabs/
│   │   ├── Unit.prefab
│   │   ├── Gate.prefab
│   │   └── EnemyBase.prefab
│   ├── Art/
│   │   └── Sprites/
│   └── Audio/
│       └── SFX/
├── ProjectSettings/
├── Packages/
└── README.md
```

---

## セットアップ手順

### 1. Unityをインストール

1. [Unity Hub](https://unity.com/download)をダウンロード
2. Unity 6 LTS（Long Term Support）をインストール - 最新は6.3 LTS
3. モジュールを含める：
   - iOS Build Support（Macの場合）
   - Android Build Support
   - WebGL Build Support（ブラウザテスト用）

### 2. プロジェクト作成

```
Unity Hub → New Project → 2D (URP) → "mob-rush"
```

最適化された2Dレンダリングのために **2D (URP)** テンプレートを選択。

### 3. プロジェクト設定

```csharp
// Edit → Project Settings → Player
Company Name: "PlanStack"
Product Name: "Mob Rush"
Default Orientation: Portrait
```

### 4. 推奨パッケージ

```
Window → Package Manager → Add:
- Input System（モダンな入力処理）
- TextMeshPro（より良いテキストレンダリング）
- 2D Sprite（2Dテンプレートに既に含まれる）
```

### 5. Gitセットアップ

```bash
# Unity用.gitignore
/[Ll]ibrary/
/[Tt]emp/
/[Oo]bj/
/[Bb]uild/
/[Bb]uilds/
/[Ll]ogs/
/[Mm]emoryCaptures/
*.csproj
*.sln
*.suo
*.user
*.pidb
*.booproj
```

---

## 基本スクリプト構造

### MonoBehaviourパターン

```csharp
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // 上に移動
        rb.velocity = Vector2.up * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Gate"))
        {
            // ゲート衝突を処理
        }
    }
}
```

### オブジェクトプーリング

```csharp
using UnityEngine;
using UnityEngine.Pool;

public class UnitSpawner : MonoBehaviour
{
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private int poolSize = 200;

    private ObjectPool<GameObject> unitPool;

    void Awake()
    {
        unitPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(unitPrefab),
            actionOnGet: obj => obj.SetActive(true),
            actionOnRelease: obj => obj.SetActive(false),
            actionOnDestroy: obj => Destroy(obj),
            maxSize: poolSize
        );
    }

    public GameObject SpawnUnit(Vector3 position)
    {
        var unit = unitPool.Get();
        unit.transform.position = position;
        return unit;
    }

    public void DespawnUnit(GameObject unit)
    {
        unitPool.Release(unit);
    }
}
```

---

## 開発ワークフロー

### Claude Codeと共に

1. **必要なものを説明** → ClaudeがC#スクリプトを生成
2. **スクリプトファイルを作成** Unity内で（Assets/Scripts/）
3. **GameObjectにアタッチ** Unity Editorで
4. **Inspectorで設定**（シリアライズされたフィールドを調整）
5. **Playボタンでテスト**
6. **結果に基づいて反復**

### シーンセットアップ（Editorで手動）

Unity Editorが必要なこと：
- GameObjectとヒエラルキーの作成
- オブジェクトへのスクリプトのアタッチ
- Prefabのセットアップ
- 物理レイヤーの設定
- シーンレイアウト

Claudeはこれらの手順をガイドできるが、クリックするのはあなた。

---

## パフォーマンス考慮事項

| 技法 | 目的 |
|------|------|
| オブジェクトプーリング | Instantiate/Destroyの代わりにユニットを再利用 |
| スプライトアトラス | 描画呼び出しを削減 |
| 物理レイヤー | 関連する衝突のみチェック |
| 固定タイムステップ | 一貫した物理（デフォルト0.02秒） |

Unityは2Dゲームのほとんどの最適化を自動的に処理。

---

## 比較まとめ

| 観点 | Phaser 3 | Unity |
|------|----------|-------|
| 言語 | TypeScript | C# |
| セットアップ | npm install | Unity Hub + Editor |
| プレビュー | ブラウザ（localhost） | Unity Playボタン |
| モバイル | Capacitorラッパー | ネイティブビルド |
| パフォーマンス | 良い（WebGL） | 優秀（ネイティブ） |
| Mob Control参照 | 間接的 | 直接（同じエンジン） |
| Claude互換性 | 高い | 高い |

**このプロジェクトの勝者: Unity**

---

## リスクと軽減策

| リスク | 軽減策 |
|--------|--------|
| Unity Editor学習曲線 | 2Dサブセットに集中、Claudeにガイドしてもらう |
| C#に不慣れ | Claudeがコードを書く、私はレビューして学ぶ |
| プロジェクトサイズが大きい | .gitignoreを適切に使う、Library/を追跡しない |
| シーン編集が手動 | 計画に手順を明確に記載 |

---

## 次のステップ

1. Unity HubとUnity 6 LTSをインストール
2. 新しい2D (URP)プロジェクトを作成
3. カメラ付きの基本シーンをセットアップ
4. 最初のスクリプト（Unit.cs）を作成
5. 単体ユニットの生成をテスト
6. `02_basic_unit_movement.md`へ進む
