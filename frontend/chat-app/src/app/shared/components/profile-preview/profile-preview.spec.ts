import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ProfilePreview } from './profile-preview';

describe('ProfilePreview', () => {
  let component: ProfilePreview;
  let fixture: ComponentFixture<ProfilePreview>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProfilePreview]
    })
      .compileComponents();

    fixture = TestBed.createComponent(ProfilePreview);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
