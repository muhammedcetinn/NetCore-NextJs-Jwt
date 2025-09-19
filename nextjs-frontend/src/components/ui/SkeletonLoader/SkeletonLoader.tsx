import styles from './SkeletonLoader.module.css';
// Basit bir iskelet yükleyici component'i
export const SkeletonText = ({ width = '100%', height = '1em' }) => (
<div className={styles.skeleton} style={{ width, height, marginBottom: '0.5rem' }} />
);
export const SkeletonCard = () => (
<div>
<SkeletonText width="40%" height="2em" />
<SkeletonText width="80%" />
<SkeletonText width="90%" />
</div>
);