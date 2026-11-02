import { TestBed } from '@angular/core/testing';
import {
  provideHttpClient
} from '@angular/common/http';
import {
  provideHttpClientTesting,
  HttpTestingController
} from '@angular/common/http/testing';

import { ProductService } from './product';

describe('ProductService', () => {
  let service: ProductService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(ProductService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get products', () => {
    service.getProducts().subscribe(response => {
      expect(response.products.length).toBe(1);
      expect(response.products[0].name).toBe('Test Product');
    });

    const request = httpMock.expectOne(
      'https://localhost:7294/api/Products?pageNumber=1&pageSize=100'
    );

    expect(request.request.method).toBe('GET');

    request.flush({
      products: [
        {
          id: 1,
          name: 'Test Product'
        }
      ],
      totalCount: 1,
      pageNumber: 1,
      pageSize: 100,
      totalPages: 1
    });
  });
});