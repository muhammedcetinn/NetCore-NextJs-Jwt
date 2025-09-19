import React from 'react';
import styles from './Button.module.css';

// Standart button props'larını ve kendi props'larımızı birleştiriyoruz
type ButtonProps = React.ButtonHTMLAttributes<HTMLButtonElement> & {
  isLoading?: boolean;
};

export default function Button({ children, isLoading, ...props }: ButtonProps) {
  return (
    <button className={styles.button} disabled={isLoading} {...props}>
      {isLoading ? <span className={styles.spinner} /> : children}
    </button>
  );
}