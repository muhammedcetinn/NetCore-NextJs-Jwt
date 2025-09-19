"use client";

import { useState } from 'react';
import { useAuth } from '@/context/AuthContext';
import { useRouter, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import Button from '@/components/ui/Button/Button'; // Yeni Button component'imiz
import styles from './LoginPage.module.css'; // Sayfaya özel stiller

export default function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [rememberMe, setRememberMe] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false); // Lokal yüklenme durumu
  const { login } = useAuth();
  const router = useRouter();
  const searchParams = useSearchParams();

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);
    setIsLoading(true); // Yüklenmeyi başlat
    
    try {
      await login({ email, password, rememberMe });
      
      const redirectUrl = searchParams.get('redirect');
      router.push(redirectUrl || '/');

    } catch (err) {
      // API Client katmanımız zaten toast gösteriyor,
      // burada sadece forma özel bir hata mesajı set ediyoruz.
      setError('E-posta veya parola hatalı.');
      setIsLoading(false); // Hata durumunda yüklenmeyi bitir
    }
    // Başarılı olursa sayfa zaten yönleneceği için setIsLoading(false) demeye gerek yok.
  };

  return (
    <div className={styles.container}>
      <div className={styles.formWrapper}>
        <h1 className={styles.title}>Giriş Yap</h1>
        <p className={styles.subtitle}>Devam etmek için bilgilerinizi girin.</p>
        
        <form onSubmit={handleSubmit} className={styles.form}>
          {error && <p className={styles.errorMessage}>{error}</p>}
          
          <div className={styles.inputGroup}>
            <label htmlFor="email">E-posta</label>
            <input
              type="email"
              id="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              className={styles.input}
            />
          </div>
          
          <div className={styles.inputGroup}>
            <label htmlFor="password">Parola</label>
            <input
              type="password"
              id="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              className={styles.input}
            />
          </div>
          
          <div className={styles.checkboxGroup}>
            <input
              type="checkbox"
              id="rememberMe"
              checked={rememberMe}
              onChange={(e) => setRememberMe(e.target.checked)}
            />
            <label htmlFor="rememberMe">Beni Hatırla</label>
          </div>
          
          <Button type="submit" isLoading={isLoading}>
            Giriş Yap
          </Button>
        </form>
        
        <p className={styles.footerText}>
          Hesabınız yok mu? <Link href="/register" className={styles.link}>Kayıt Olun</Link>
        </p>
      </div>
    </div>
  );
}