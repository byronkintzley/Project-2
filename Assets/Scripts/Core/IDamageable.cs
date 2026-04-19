namespace Project2
{
    /// <summary>
    /// Anything the player's weapon (or a hazard) can damage implements this.
    /// Keeps PlayerShoot / Projectile decoupled from concrete Enemy / Player classes.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(float amount);
    }
}
