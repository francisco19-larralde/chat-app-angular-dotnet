import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { App } from './app';
import { routes } from './app.routes';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideZonelessChangeDetection()]
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should protect the chat layout and keep login public-only', () => {
    const login = routes.find(route => route.path === 'login');
    const layout = routes.find(route => route.path === '');

    expect(login?.canActivate?.length).toBe(1);
    expect(layout?.canActivate?.length).toBe(1);
    expect(layout?.children?.some(route => route.path === 'chat/:chatId')).toBeTrue();
  });
});
