"use client";

import Link from 'next/link';
import { useAuth } from '@/context/AuthContext';
import { useRouter } from 'next/navigation';
import styles from './Navbar.module.css';
import Button from '../ui/Button/Button';

/**
 * Kullanıcının kimlik doğrulama durumuna göre değişen
 * Login veya Logout/Hoş Geldiniz mesajını gösteren alt component.
 */
const AuthNavSection = () => {
  const { user, logout, isLoading } = useAuth();
  const router = useRouter();

  const handleLogout = async () => {
    await logout();
    // Yönlendirme, logout fonksiyonunun kendisi tarafından yönetilebilir,
    // ancak burada olması da kabul edilebilir.
    router.push('/login');
  };

  if (isLoading) {
    return <span className={styles.loadingText}>Yükleniyor...</span>;
  }

  if (user) {
    return (
      <>
        <span className={styles.welcomeText}>Hoş geldin, {user.email}!</span>
        <Button onClick={handleLogout} className={styles.button} isLoading={isLoading}>Logout</Button>
      </>
    );
  }

  return <Link href="/login">Login</Link>;
};

/**
 * Uygulamanın ana gezinti çubuğu (Navbar).
 * Kullanıcının rollerine göre gezinti linklerini dinamik olarak gösterir.
 */
export default function Navbar() {
  const { user } = useAuth(); // Sadece 'user' bilgisine ihtiyacımız var

  // Rol kontrolünü burada yapmak, render bloğunu temiz tutar.
  const isAdmin = user?.roles.some(role => role.toLowerCase() === 'admin');

  return (
    <header className={styles.navbar}>
      <nav className={styles.navLinks}>
        <Link href="/">Home</Link>
        {/* Giriş yapmış herhangi bir kullanıcı bu linki görebilir. */}
        {user && <Link href="/user">User Page</Link>}
        {/* Sadece admin rolüne sahip kullanıcılar bu linki görebilir. */}
        {isAdmin && <Link href="/admin">Admin Page</Link>}
      </nav>
      <div className={styles.authLinks}>
        <AuthNavSection />
      </div>
    </header>
  );
}