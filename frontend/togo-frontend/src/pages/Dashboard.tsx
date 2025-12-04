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
    { label: "Dashboard", path: "/dashboard", icon: "/dashboard.png" },
    { label: "Clientes",  path: "/clientes",  icon: "/cliente.png" },
    { label: "Pets",      path: "/pets",      icon: "/pet.png" },
    { label: "Agenda",    path: "/agenda",    icon: "/agenda.png" },
    { label: "Atendimentos", path: "/atendimentos", icon: "/atendimento.png" },
    { label: "Vacinas",   path: "/vacinas",   icon: "/vacina.png" },
    { label: 'Cadastro', path: '/cadastro', icon: '/cadastro.png' },
    { label: 'Financeiro', path: '/financeiro', icon: '/financeiro.png' },
    { label: 'Relatórios', path: '/relatorios', icon: '/relatorio.png' },
    { label: 'Configurações', path: '/configuracoes', icon: '/configuracoes.png' },
    { label: 'PDV', path: '/pdv' },
  ];

  return (
    <div className="dashboard-container">
      {/* Barra lateral */}
      <aside className="sidebar">
       <div
    className="sidebar-logo"
    onClick={() => navigate('/dashboard')}
  >
    <img
      src="/vite.png"
      alt="TOGO Logo"
      className="sidebar-logo-icon"
    />
    <span className="sidebar-logo-text">TOGO</span>
  </div>
        <ul className="nav-list">
          {navItems.map((item) => (
            <li
              key={item.path}
              onClick={() => navigate(item.path)}
              className="nav-list-item"
            >
              <div className="nav-item-content">
                {item.icon && (
                  <img
                    src={item.icon}
                    alt={item.label}
                    className="nav-icon"
                  />
                )}
                <span>{item.label}</span>
              </div>
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
            <div className="metric-card-header">
              <img
                src="/agenda.png"
                alt="Agendamentos Hoje"
                className="metric-icon"
              />
              <h3>Agendamentos Hoje</h3>
            </div>
            <div className="value">{stats.agendamentosHoje}</div>
            <span className="description">
              {stats.agendamentosSemana} esta semana
            </span>
          </div>
          <div className="metric-card">
            <div className="metric-card-header">
             <img
                src="/financeiro.png"
                alt="Faturamento Hoje"
                className="metric-icon"
              />
            <h3>Faturamento Hoje</h3>
            </div>
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
<div className="metric-card-header">
               <img
                src="/cliente.png"
                alt="Clientes Ativos"
                className="metric-icon"
              />
            <h3>Clientes Ativos</h3>
            </div>
            <div className="value">{stats.totalClientes}</div>
            <span className="description">
              {stats.totalPets} pets cadastrados
            </span>
          </div>
          <div className="metric-card">
              <div className="metric-card-header">
             <img
                src="/alerta.png"
                alt="Alertas de Estoque"
                className="metric-icon"
              />
            <h3>Alertas de Estoque</h3>
            </div>
            <div className="value">{stats.produtosBaixoEstoque}</div>
            <span className="description">Produtos abaixo do mínimo</span>
          </div>
        </div>

        {/* Métricas secundárias */}
        <div className="metrics-grid">
          <div className="metric-card">
            <div className="metric-card-header">
             <img
                src="/grafico.png"
                alt="Taxa de Ocupação Semanal"
                className="metric-icon"
              />
            <h3>Taxa de Ocupação Semanal</h3>
            </div>
            <div className="value">{stats.taxaOcupacao}%</div>
            <span className="description">
              Baseado em {stats.agendamentosSemana} agendamentos
            </span>
          </div>
          <div className="metric-card">
            <div className="metric-card-header">
             <img
                src="/vacina.png"
                alt="Vacinas Pendentes"
                className="metric-icon"
              />
            <h3>Vacinas Pendentes</h3>
            </div>
            <div className="value">{stats.vacinasPendentes}</div>
            <span className="description">
              Próximos 7 dias - Enviar lembretes
            </span>
          </div>
          <div className="metric-card">
            <div className="metric-card-header">
             <img
                src="/gestao.png"
                alt="Gestão de Estoque"
                className="metric-icon"
              />
            <h3>Gestão de Estoque</h3>
            </div>
            <div className="value">{stats.produtosBaixoEstoque}</div>
            <span className="description">Requer reposição urgente</span>
          </div>
        </div>
      </main>
    </div>
  );
};


export default Dashboard;