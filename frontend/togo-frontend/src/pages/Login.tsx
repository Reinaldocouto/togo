import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import api from '../services/api';
import { useAuth, User } from '../contexts/AuthContext';

interface LoginResponse {
  userId: string;
  name: string;
  email: string;
  token: string;
}

const Login: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const navigate = useNavigate();
  const { login } = useAuth();

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      const { data } = await api.post('/Auth/login', { email, password });
      const { name, email: userEmail, token } = data;

      if (!token || !name || !userEmail) {
        throw new Error('Resposta de login inválida');
      }

      login({ name, email: userEmail }, token);
      navigate('/dashboard');
    } catch (err) {
       const isUnauthorized = axios.isAxiosError(err) && err.response?.status === 401;
      const message = isUnauthorized
        ? 'Usuário ou senha inválidos'
        : 'Não foi possível realizar o login. Tente novamente.';
      setError(message);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="app-shell">
      <div className="card">
        <h1>TOGO</h1>
        <form onSubmit={handleSubmit}>
          <h2>Login</h2>
          <input
            type="email"
            placeholder="Email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
          <input
            type="password"
            placeholder="Senha"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
          {error ? <div className="error">{error}</div> : null}
          <button type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Entrando...' : 'Entrar'}
          </button>
        </form>
      </div>
    </div>
  );
};

export default Login;
