using UnityEngine;

public class ConflictPlayerFinder : MonoBehaviour
{
    int damage;
    public void Init(int damage)
    {
        this.damage = damage;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        var conflictEnemy = other.gameObject.GetComponent<IConflictEnemy>();
        if (conflictEnemy == null) return;
        conflictEnemy.OnConflictEnemy(damage);
    }

    //今はボスの出す攻撃にダメージを載せようとしているんだけど、攻撃はisTriggerにしているので、OnTriggerEnter2Dを使いたいけど、オーバーライドとかできないから、他のスクリプト作るか、Bodyに直接書く、または、インターフェースを書いて効率化するか、どうしようかな？？？？
}
