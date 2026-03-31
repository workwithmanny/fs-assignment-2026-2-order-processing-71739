import { Home } from "./components/Home";
import { OrdersDashboard } from "./components/OrdersDashboard";
import { OrderDetails } from "./components/OrderDetails";

const AppRoutes = [
  {
    index: true,
    element: <OrdersDashboard />
  },
  {
    path: '/order-details/:id',
    element: <OrderDetails />
  }
];

export default AppRoutes;