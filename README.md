# Form Recognizer (Preview) のテストプロジェクト

## カスタムデータのトレーニング

https://docs.microsoft.com/ja-jp/azure/cognitive-services/form-recognizer/build-training-data-set

### トレーニングデータのアップロード

※[一般的な入力の要件](https://docs.microsoft.com/ja-jp/azure/cognitive-services/form-recognizer/build-training-data-set#general-input-requirements) に注意してBlobストレージに学習用データを[アップロード](https://docs.microsoft.com/ja-jp/azure/storage/blobs/storage-quickstart-blobs-portal)していく。

1. ストレージアカウント(BlobStorage)を作成する
2. ストレージアカウントでコンテナを作る
3. コンテナに学習用データをアップロードする


### コンテナーの共有アクセス署名 URL を発行する

事前にサブスクリプションのアクセス制御(IAM)でSASを発行するためのロールを割り当てます。

1. サブスクリプションの アクセス制御 (IAM) で ロールの割り当て を選択します。
2. 「ストレージ BLOB データ共同作成者」を発行したいメールアドレスに割り当てます。
3. PowerShellで[SASトークンを発行します](https://docs.microsoft.com/ja-jp/azure/storage/blobs/storage-blob-user-delegation-sas-create-powershell)。

※カスタムモデルの学習では コンテナのURL + SASトークン のURLを指定します。
URLはコンテナのプロパティで確認できます。

## カスタムモデルを使う

https://docs.microsoft.com/ja-jp/azure/cognitive-services/form-recognizer/quickstarts/dotnet-sdk

