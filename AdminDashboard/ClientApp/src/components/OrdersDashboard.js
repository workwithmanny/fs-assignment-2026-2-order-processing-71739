import React, { useState, useEffect } from 'react';

const API_BASE = 'http://localhost:5001';

export function OrdersDashboard() {
    const [orders, setOrders] = useState([]);
    const [loading, setLoading] = useState(true);
    const [statusFilter, setStatusFilter] = useState('');

    useEffect(() => {
        fetchOrders();
        const interval = setInterval(fetchOrders, 10000);
        return () => clearInterval(interval);
    }, []);

    async function fetchOrders() {
        try {
            const response = await fetch(`${API_BASE}/api/orders`);
            if (response.ok) {
                const data = await response.json();
                setOrders(data);
            }
        } catch (err) {
            console.error('Error fetching orders:', err);
        } finally {
            setLoading(false);
        }
    }

    const filtered = statusFilter
        ? orders.filter(o => o.status === statusFilter)
        : orders;

    const statuses = [...new Set(orders.map(o => o.status))];

    const total = orders.length;
    const completed = orders.filter(o => o.status === 'Completed').length;
    const failed = orders.filter(o => o.status === 'Failed').length;
    const pending = orders.filter(o => !['Completed', 'Failed'].includes(o.status)).length;

    if (loading) return <p>Loading orders...</p>;

    return (
        <div>
            <h2>Admin Dashboard</h2>

            <div style={{ display: 'flex', gap: '16px', marginBottom: '24px' }}>
                <StatCard title="Total Orders" value={total} color="#007bff" />
                <StatCard title="Completed" value={completed} color="#28a745" />
                <StatCard title="Failed" value={failed} color="#dc3545" />
                <StatCard title="Pending" value={pending} color="#ffc107" />
            </div>

            <div style={{ marginBottom: '16px' }}>
                <label>Filter by status: </label>
                <select value={statusFilter} onChange={e => setStatusFilter(e.target.value)}>
                    <option value="">All</option>
                    {statuses.map(s => <option key={s} value={s}>{s}</option>)}
                </select>
                <button onClick={fetchOrders} style={{ marginLeft: '8px' }}>Refresh</button>
            </div>

            {filtered.length === 0 ? (
                <p>No orders found.</p>
            ) : (
                <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                    <thead>
                    <tr style={{ backgroundColor: '#f8f9fa' }}>
                        <th style={thStyle}>Order ID</th>
                        <th style={thStyle}>Customer</th>
                        <th style={thStyle}>Total</th>
                        <th style={thStyle}>Status</th>
                        <th style={thStyle}>Date</th>
                        <th style={thStyle}>Actions</th>
                    </tr>
                    </thead>
                    <tbody>
                    {filtered.map(order => (
                        <tr key={order.id} style={{ borderBottom: '1px solid #dee2e6' }}>
                            <td style={tdStyle}>{order.id?.substring(0, 8)}...</td>
                            <td style={tdStyle}>{order.customerName}</td>
                            <td style={tdStyle}>€{order.totalAmount?.toFixed(2)}</td>
                            <td style={tdStyle}>
                                <StatusBadge status={order.status} />
                            </td>
                            <td style={tdStyle}>{new Date(order.createdAt).toLocaleDateString()}</td>
                            <td style={tdStyle}>
                                <a href={`/order-details/${order.id}`}>View</a>
                            </td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            )}
        </div>
    );
}

function StatCard({ title, value, color }) {
    return (
        <div style={{
            padding: '16px', borderRadius: '8px', backgroundColor: color,
            color: 'white', minWidth: '120px', textAlign: 'center'
        }}>
            <h3 style={{ margin: 0 }}>{value}</h3>
            <p style={{ margin: 0 }}>{title}</p>
        </div>
    );
}

function StatusBadge({ status }) {
    const colors = {
        'Completed': '#28a745',
        'Failed': '#dc3545',
        'Submitted': '#007bff',
        'PaymentApproved': '#17a2b8',
        'InventoryConfirmed': '#6f42c1',
    };
    const color = colors[status] || '#6c757d';
    return (
        <span style={{
            backgroundColor: color, color: 'white',
            padding: '2px 8px', borderRadius: '4px', fontSize: '12px'
        }}>
            {status}
        </span>
    );
}

const thStyle = { padding: '8px', textAlign: 'left', borderBottom: '2px solid #dee2e6' };
const tdStyle = { padding: '8px' };