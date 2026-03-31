import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';

const API_BASE = 'http://localhost:5001';

export function OrderDetails() {
    const { id } = useParams();
    const [order, setOrder] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        fetchOrder();
    }, [id]);

    async function fetchOrder() {
        try {
            const response = await fetch(`${API_BASE}/api/orders/${id}`);
            if (response.ok) {
                const data = await response.json();
                setOrder(data);
            }
        } catch (err) {
            console.error('Error fetching order:', err);
        } finally {
            setLoading(false);
        }
    }

    if (loading) return <p>Loading order...</p>;
    if (!order) return <p>Order not found.</p>;

    return (
        <div>
            <h2>Order Details</h2>
            <a href="/">← Back to Dashboard</a>

            <div style={{ marginTop: '16px', padding: '16px', border: '1px solid #dee2e6', borderRadius: '8px' }}>
                <p><strong>Order ID:</strong> {order.id}</p>
                <p><strong>Customer:</strong> {order.customerName}</p>
                <p><strong>Email:</strong> {order.customerEmail}</p>
                <p><strong>Address:</strong> {order.shippingAddress}</p>
                <p><strong>Status:</strong> {order.status}</p>
                <p><strong>Total:</strong> €{order.totalAmount?.toFixed(2)}</p>
                <p><strong>Date:</strong> {new Date(order.createdAt).toLocaleString()}</p>
                <p><strong>Correlation ID:</strong> {order.correlationId}</p>
            </div>

            <h4 style={{ marginTop: '16px' }}>Order Items</h4>
            <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                <thead>
                <tr style={{ backgroundColor: '#f8f9fa' }}>
                    <th style={{ padding: '8px', textAlign: 'left' }}>Product</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>Qty</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>Unit Price</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>Subtotal</th>
                </tr>
                </thead>
                <tbody>
                {order.items?.map((item, i) => (
                    <tr key={i} style={{ borderBottom: '1px solid #dee2e6' }}>
                        <td style={{ padding: '8px' }}>{item.productName}</td>
                        <td style={{ padding: '8px' }}>{item.quantity}</td>
                        <td style={{ padding: '8px' }}>€{item.unitPrice?.toFixed(2)}</td>
                        <td style={{ padding: '8px' }}>€{(item.unitPrice * item.quantity)?.toFixed(2)}</td>
                    </tr>
                ))}
                </tbody>
            </table>

            {order.payment && (
                <div style={{ marginTop: '16px', padding: '16px', border: '1px solid #dee2e6', borderRadius: '8px' }}>
                    <h4>Payment Info</h4>
                    <p><strong>Transaction ID:</strong> {order.payment.transactionId}</p>
                    <p><strong>Status:</strong> {order.payment.status}</p>
                    {order.payment.failureReason && <p><strong>Failure Reason:</strong> {order.payment.failureReason}</p>}
                </div>
            )}

            {order.shipment && (
                <div style={{ marginTop: '16px', padding: '16px', border: '1px solid #dee2e6', borderRadius: '8px' }}>
                    <h4>Shipment Info</h4>
                    <p><strong>Reference:</strong> {order.shipment.shipmentReference}</p>
                    <p><strong>Estimated Dispatch:</strong> {new Date(order.shipment.estimatedDispatchDate).toLocaleDateString()}</p>
                </div>
            )}
        </div>
    );
}