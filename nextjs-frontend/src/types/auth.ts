// Backend'deki UserDto'ya karşılık gelen tip.
export interface User {
  id: string;
  email: string;
  roles: string[];
}

// Login ve getCurrentUser'dan dönen tam cevap.
export interface UserAuthResponse extends User {
  csrfToken: string;
}

// Login API'sine gönderilen veri.
export interface LoginCredentials {
  email: string;
  password: string;
  rememberMe: boolean;
}