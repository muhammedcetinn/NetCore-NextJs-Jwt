"use client";
import { useUiStore } from '@/store/uiStore';
import styles from './GlobalSpinner.module.css';

export default function GlobalSpinner() {
    const isApiLoading = useUiStore((state) => state.isApiLoading);

    if (!isApiLoading) {
        return null;
    }

    return (
        <div className={styles.overlay}>
            <div className={styles.spinner}></div>
        </div>
    );
}