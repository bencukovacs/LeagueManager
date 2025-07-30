import { Outlet } from 'react-router-dom';
import Navbar from '../components/Navbar';

export default function RootLayout() {
  return (
    <div>
      <Navbar />
      <main>
        {/* The Outlet component will render the current page's content */}
        <Outlet />
      </main>
    </div>
  );
}