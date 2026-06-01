import { useState, useCallback } from 'react';
import type { FC, FormEvent } from 'react';
import { useAuth } from '../../contexts/AuthContext';
import './AuthView.css';

export const AuthView: FC = () => {
  const [isLogin, setIsLogin] = useState(true);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  
  // Cadastro
  const [companyName, setCompanyName] = useState('');
  const [companyDocument, setCompanyDocument] = useState('');
  const [adminName, setAdminName] = useState('');
  
  const { login, register } = useAuth();
  const [loading, setLoading] = useState(false);

  const handleSubmit = useCallback(async (e: FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      if (isLogin) {
        await login(email, password);
      } else {
        await register({
          companyName,
          document: companyDocument,
          adminName,
          email,
          password,
        });
      }
    } catch (err: unknown) {
      const apiError = err as { message?: string };
      alert(apiError.message || 'Erro ao autenticar. Verifique os dados.');
    } finally {
      setLoading(false);
    }
  }, [isLogin, email, password, companyName, companyDocument, adminName, login, register]);

  const toggleMode = useCallback(() => {
    setIsLogin((prev) => !prev);
  }, []);

  return (
    <div className="auth-view-overlay">
      <div className="auth-card glass-card">
        <div className="auth-header">
          🏢 <h2>B2B Marketplace Portal</h2>
          <p>Gerencie compras e suprimentos corporativos em escala</p>
        </div>

        <form onSubmit={handleSubmit} className="auth-form">
          {!isLogin && (
            <>
              <div className="form-group">
                <label>Razão Social da Empresa</label>
                <input 
                  type="text" 
                  required 
                  value={companyName} 
                  onChange={(e) => setCompanyName(e.target.value)} 
                  placeholder="Ex: Construtora Alvorada Ltda" 
                />
              </div>
              <div className="form-group">
                <label>CNPJ / Identificação</label>
                <input 
                  type="text" 
                  required 
                  value={companyDocument} 
                  onChange={(e) => setCompanyDocument(e.target.value)} 
                  placeholder="Ex: 12.345.678/0001-99" 
                />
              </div>
              <div className="form-group">
                <label>Nome Completo do Administrador</label>
                <input 
                  type="text" 
                  required 
                  value={adminName} 
                  onChange={(e) => setAdminName(e.target.value)} 
                  placeholder="Ex: João da Silva" 
                />
              </div>
            </>
          )}

          <div className="form-group">
            <label>E-mail Corporativo</label>
            <input 
              type="email" 
              required 
              value={email} 
              onChange={(e) => setEmail(e.target.value)} 
              placeholder="admin@empresa.com" 
            />
          </div>

          <div className="form-group">
            <label>Senha de Segurança</label>
            <input 
              type="password" 
              required 
              value={password} 
              onChange={(e) => setPassword(e.target.value)} 
              placeholder="Senha (min 6 caracteres)" 
            />
          </div>

          <button type="submit" className="btn-primary auth-submit-btn" disabled={loading}>
            {loading ? 'Processando...' : isLogin ? 'Acessar Conta B2B' : 'Cadastrar Empresa'}
          </button>
        </form>

        <div className="auth-switcher">
          <button className="switch-btn" onClick={toggleMode}>
            {isLogin ? 'Nova empresa? Cadastre-se aqui' : 'Já possui conta? Faça o login corporativo'}
          </button>
        </div>
      </div>
    </div>
  );
};
