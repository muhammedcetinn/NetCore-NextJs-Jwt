import React from 'react';
import styles from './InfoBox.module.css';

interface InfoBoxProps {
  title: string;
  children: React.ReactNode;
  // Renkleri daha kontrollü hale getirelim
  variant?: 'default' | 'primary' | 'danger'; 
}

export default function InfoBox({ title, children, variant = 'default' }: InfoBoxProps) {
  // CSS modülünden ilgili sınıf adını dinamik olarak al
  const boxClassName = `${styles.infoBox} ${styles[variant]}`;

  return (
    <div className={boxClassName}>
      <h2 className={styles.title}>{title}</h2>
      <div className={styles.content}>
        {children}
      </div>
    </div>
  );
}