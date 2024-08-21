# ShaderPropertyClassGenerator
UnityのShaderのPropertyからクラスを自動生成するEditor拡張

# 導入方法
* unitypackageをダウンロードして使う

# 使い方
![image](https://github.com/user-attachments/assets/b6216da6-da33-4701-9d6e-b1b6f52cbbf6)<br>
Shaderを選択した状態で右クリック、
Create >ShaderPropertyClass

![image](https://github.com/user-attachments/assets/a02fe432-f379-4124-9149-5020e3debd62)<br>
専用Windowが出ます。

**クリップボードに保存**を押すことでクリップボードに保存できます。

**クラスを保存**を押すことでこの変数を定義したクラスを生成します。生成場所はShaderのデータと同階層に生成します。
```C#
using UnityEngine;
public static class ColorDivedProperty
{
    public static readonly int MAIN_TEX_ID = Shader.PropertyToID("_MainTex");
    public static readonly int DIVED_MODE_ID = Shader.PropertyToID("_DivedMode");

}
```
例としてこのようなクラスが作成されます。

# 不具合
未確認
