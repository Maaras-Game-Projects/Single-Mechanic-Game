
namespace EternalKeep
{
    public interface IDamagable
    {
        void TakeDamage(float damageAmount);
        void TakeDamage(float damageAmount, float criticalDamage);
    }

}


