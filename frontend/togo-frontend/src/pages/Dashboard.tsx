import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

const Dashboard: React.FC = () => {
  const { user, token, logout } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    if (!token) {
      navigate('/login', { replace: true });
    }
  }, [token, navigate]);

  const handleLogout = () => {
    logout();
    navigate('/login', { replace: true });
  };

  return (
    <div className="app-shell">
      <div className="card">
        <h1>Dashboard</h1>
        <p>Bem-vindo, {user?.name ?? 'usuário'}!</p>
        <button type="button" onClick={handleLogout}>
          Sair
        </button>
      </div>
    </div>
  );
};

export default Dashboard;
