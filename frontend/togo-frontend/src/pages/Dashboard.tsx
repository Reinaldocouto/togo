import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import './Dashboard.css';


const Dashboard: React.FC = () => {
  const { user, token, logout } = useAuth();
  const navigate = useNavigate();

 
  const [stats, setStats] = useState({
    agendamentosHoje: 0,
    agendamentosSemana: 0,
    faturamentoHoje: 0,
    faturamento30d: 0,
    totalClientes: 0,
    totalPets: 0,
    produtosBaixoEstoque: 0,
    vacinasPendentes: 0,
    taxaOcupacao: 0,
  });

 
  useEffect(() => {
    if (!token) {
      navigate('/login', { replace: true });
    }
  }, [token, navigate]);

 
  const getGreeting = (): string => {
    const hour = new Date().getHours();
    if (hour < 12) return 'Bom dia';
    if (hour < 18) return 'Boa tarde';
    return 'Boa noite';
  };

 
  const handleLogout = () => {
    logout();
    navigate('/login', { replace: true });
  };

 
  const navItems = [
    { label: 'Dashboard', path: '/dashboard' },
    { label: 'Clientes', path: '/clientes' },
    { label: 'Pets', path: '/pets' },
    { label: 'Agenda', path: '/agenda' },
    { label: 'Atendimentos', path: '/atendimentos' },
    { label: 'Vacinas', path: '/vacinas' },
    { label: 'Produtos', path: '/produtos' },
    { label: 'PDV', path: '/pdv' },
    { label: 'Financeiro', path: '/financeiro' },
    { label: 'Relatórios', path: '/relatorios' },
    { label: 'Configurações', path: '/configuracoes' },
  ];

  return (
    <div className="dashboard-container">
      {/* Barra lateral */}
      <aside className="sidebar">
        <div
            className="logo"
              onClick={() => navigate('/dashboard')}
              >TOGO
        </div>
        <ul className="nav-list">
          {navItems.map((item) => (
            <li key={item.path} onClick={() => navigate(item.path)}>
              {item.label}
            </li>
          ))}
        </ul>
      </aside>

      {/* Conteúdo principal */}
      <main className="content">
        <div className="topbar">
          <span className="greeting">
            {getGreeting()} {user?.name ? `Dr ${user.name}` : ''}
          </span>
          <button type="button" onClick={handleLogout}>
            Sair
          </button>
        </div>
        <h1>Dashboard</h1>
        <p>Visão geral da sua clínica veterinária</p>

        {/* Métricas principais */}
        <div className="metrics-grid">
          <div className="metric-card">
            <h3>Agendamentos Hoje</h3>
            <div className="value">{stats.agendamentosHoje}</div>
            <span className="description">
              {stats.agendamentosSemana} esta semana
            </span>
          </div>
          <div className="metric-card">
            <h3>Faturamento Hoje</h3>
            <div className="value">
              {stats.faturamentoHoje.toLocaleString('pt-BR', {
                style: 'currency',
                currency: 'BRL',
              })}
            </div>
            <span className="description">
              {stats.faturamento30d.toLocaleString('pt-BR', {
                style: 'currency',
                currency: 'BRL',
              })}{' '}
              em 30 dias
            </span>
          </div>
          <div className="metric-card">
            <h3>Clientes Ativos</h3>
            <div className="value">{stats.totalClientes}</div>
            <span className="description">
              {stats.totalPets} pets cadastrados
            </span>
          </div>
          <div className="metric-card">
            <h3>Alertas de Estoque</h3>
            <div className="value">{stats.produtosBaixoEstoque}</div>
            <span className="description">Produtos abaixo do mínimo</span>
          </div>
        </div>

        {/* Métricas secundárias */}
        <div className="metrics-grid">
          <div className="metric-card">
            <h3>Taxa de Ocupação Semanal</h3>
            <div className="value">{stats.taxaOcupacao}%</div>
            <span className="description">
              Baseado em {stats.agendamentosSemana} agendamentos
            </span>
          </div>
          <div className="metric-card">
            <h3>Vacinas Pendentes</h3>
            <div className="value">{stats.vacinasPendentes}</div>
            <span className="description">
              Próximos 7 dias - Enviar lembretes
            </span>
          </div>
          <div className="metric-card">
            <h3>Gestão de Estoque</h3>
            <div className="value">{stats.produtosBaixoEstoque}</div>
            <span className="description">Requer reposição urgente</span>
          </div>
        </div>
      </main>
    </div>
  );
};

export default Dashboard;